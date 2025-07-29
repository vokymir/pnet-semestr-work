import csv
import time
import requests
from bs4 import BeautifulSoup
import re # Import the regular expression module

def get_bird_taxonomy_local(genus, species):
    """
    Searches Wikipedia for the family and order of a given bird genus and species.
    This version is designed for local execution using requests and direct Wikipedia URLs.

    Args:
        genus (str): The genus name of the bird.
        species (str): The species name of the bird.

    Returns:
        tuple: A tuple (family, order) or (None, None) if not found.
    """
    # Construct potential Wikipedia URLs with improved capitalization for Czech and English
    # For Czech Wikipedia, typically only the first word (genus) is capitalized.
    czech_formatted_name = f"{genus.capitalize()}_{species.lower()}"

    # For English Wikipedia, often all major words are capitalized.
    english_formatted_name = ' '.join([word.capitalize() for word in f"{genus} {species}".split()]).replace(' ', '_')

    wikipedia_urls_to_try = [
        f"https://cs.wikipedia.org/wiki/{czech_formatted_name}", # Prioritize correct Czech format
        f"https://en.wikipedia.org/wiki/{english_formatted_name}", # Then try English format
        # Fallback patterns in case the primary ones don't match
        f"https://cs.wikipedia.org/wiki/{english_formatted_name}", # Try English-style capitalization on Czech wiki
        f"https://en.wikipedia.org/wiki/{czech_formatted_name}" # Try Czech-style capitalization on English wiki
    ]

    page_content = None
    target_url = None

    for url in wikipedia_urls_to_try:
        try:
            print(f"  Trying URL: {url}")
            response = requests.get(url, timeout=10) # 10-second timeout
            response.raise_for_status() # Raise HTTPError for bad responses (4xx or 5xx)

            soup_check = BeautifulSoup(response.text, 'html.parser')

            # --- Improved Page Validation Logic ---
            # 1. Check the page title for "non-existent" indicators
            page_title_tag = soup_check.find('title')
            if page_title_tag:
                page_title = page_title_tag.get_text(strip=True).lower()
                # Common "page does not exist" patterns in titles
                if "neexistuje" in page_title or "not found" in page_title or "does not exist" in page_title:
                    print(f"  URL {url} title indicates non-existence, trying next.")
                    continue

            # 2. Check for disambiguation pages by looking at the URL and common disambiguation indicators in content
            # 'rozcestník' is common on Czech Wikipedia for disambiguation
            # 'disambiguation' is common on English Wikipedia
            content_text_div = soup_check.find('div', id='mw-content-text')
            if "disambiguation" in response.url or \
               (content_text_div and ("rozcestník" in content_text_div.get_text().lower() or \
                                     "disambiguation" in content_text_div.get_text().lower())):
                print(f"  URL {url} appears to be a disambiguation page, trying next.")
                continue

            # 3. Final check: Does the page contain an infobox or a main heading (h1)?
            # These are strong indicators of a valid content page for a species.
            if not soup_check.find('table', class_='infobox') and not soup_check.find('h1', class_='firstHeading'):
                print(f"  URL {url} does not appear to be a main content page (missing infobox/h1), trying next.")
                continue
            # --- End of Improved Page Validation Logic ---

            page_content = response.text
            target_url = url
            break # Found a valid page, stop trying URLs

        except requests.exceptions.RequestException as e:
            print(f"  Request error for {url}: {e}")
            continue # Try the next URL

    if not page_content:
        print(f"  Could not find a suitable Wikipedia page for {genus} {species}.")
        return None, None

    print(f"  Successfully fetched content from: {target_url}")

    # Parse the HTML content to extract Family and Order
    soup = BeautifulSoup(page_content, 'html.parser')
    family = None
    order = None

    # Look for infobox tables, common in Wikipedia for taxonomy
    infobox = soup.find('table', class_='infobox')
    if infobox:
        rows = infobox.find_all('tr')
        for row in rows:
            header = row.find('th')
            data = row.find('td')
            if header and data:
                header_text = header.get_text(strip=True).lower() # Convert to lowercase for robust comparison
                data_text = data.get_text(strip=True)

                # Look for Czech and English terms, and handle potential trailing colons or spaces
                if "čeleď" in header_text or "family" in header_text:
                    family = data_text
                elif "řád" in header_text or "order" in header_text:
                    order = data_text

            # If both are found, no need to continue searching
            if family and order:
                break

    # Clean up extracted names (e.g., remove [citation needed] or other annotations)
    if family:
        family = re.sub(r'\s*\(.*\)', '', family).strip() # Remove text in parentheses and strip whitespace
        family = family.split('[')[0].strip() # Remove citation brackets
    if order:
        order = re.sub(r'\s*\(.*\)', '', order).strip() # Remove text in parentheses and strip whitespace
        order = order.split('[')[0].strip() # Remove citation brackets

    return family, order

def process_bird_csv(input_csv_path, output_csv_path):
    """
    Reads bird names from an input CSV, searches for their taxonomy using local methods,
    and writes each processed record to a new output CSV immediately.
    """
    try:
        with open(input_csv_path, 'r', newline='', encoding='utf-8') as infile:
            reader = csv.reader(infile)
            header = next(reader, None) # Read header

            with open(output_csv_path, 'w', newline='', encoding='utf-8') as outfile:
                writer = csv.writer(outfile)

                output_header = header + ['Family', 'Order'] if header else ['Genus', 'Species', 'Family', 'Order']
                writer.writerow(output_header) # Write header immediately

                for i, row in enumerate(reader):
                    if len(row) >= 2:
                        genus = row[0].strip()
                        species = row[1].strip()

                        print(f"Processing bird {i+1}: {genus} {species}")
                        family, order = get_bird_taxonomy_local(genus, species)

                        output_row = row + [family if family else '', order if order else '']
                        writer.writerow(output_row) # Write each row immediately
                        outfile.flush() # Ensure data is written to disk

                        # Print the found family and order to console
                        print(f"  Found: Family='{family if family else 'N/A'}', Order='{order if order else 'N/A'}'")

                        time.sleep(1.5) # Increased delay to be polite to Wikipedia servers
                    else:
                        print(f"Skipping malformed row: {row}")
                        writer.writerow(row + ['', '']) # Write malformed row with empty columns
                        outfile.flush() # Ensure data is written to disk

        print(f"\nSuccessfully processed data and saved to '{output_csv_path}'")

    except FileNotFoundError:
        print(f"Error: The input file '{input_csv_path}' was not found.")
    except Exception as e:
        print(f"An unexpected error occurred: {e}")

if __name__ == "__main__":
    input_csv = 'czech_bird_names.csv' # Using the output from the previous script
    output_csv = 'czech_bird_taxonomy_full_local.csv' # New output filename to avoid overwriting

    process_bird_csv(input_csv, output_csv)
