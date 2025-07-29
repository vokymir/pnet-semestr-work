from bs4 import BeautifulSoup
import csv

def extract_czech_bird_names_from_html_refined(html_content):
    """
    Extracts Czech bird names from HTML content, handling specific inclusions and exclusions.

    Args:
        html_content (str): The full HTML content of the webpage.

    Returns:
        list: A list of dictionaries, where each dictionary represents a bird
              with 'Genus' and 'Species' keys.
    """
    bird_names = []
    valid_bird_count = 0
    max_valid_birds = 400

    soup = BeautifulSoup(html_content, 'html.parser')

    # Find all list items that represent bird entries
    list_items = soup.find_all('li', class_='BirdList-list-list-item')

    for list_item in list_items:
        common_name_span = list_item.find('span', class_='Species-common')
        if not common_name_span:
            continue # Skip if no common name span is found

        full_name = common_name_span.get_text(strip=True)

        # --- Exclusion Logic for Hybridi and Další taxony ---
        # Check for the 'Species--nonSpeciesTaxa' class or specific keywords
        is_non_species_taxa = list_item.find('span', class_='Species--nonSpeciesTaxa') is not None
        if is_non_species_taxa or " (hybrid)" in full_name or "další taxonomie" in full_name:
            continue # Skip this entry if it's a hybrid or other non-species taxa

        # --- Check for "EXOTI" category (explicit inclusion) ---
        # Identified by the presence of an exotic button/icon within the bird's list item
        is_exotic = list_item.find('button', class_='Obs-species-exotic') is not None

        # --- Process the bird name ---
        processed_entries = []
        parts = full_name.split(' ', 1)
        genus = parts[0]
        species_part = parts[1] if len(parts) > 1 else ''

        if '/' in species_part:
            # Split and process multiple species names if a '/' is present
            sub_species_parts = species_part.split('/')
            for sub_sp in sub_species_parts:
                processed_entries.append({'Genus': genus, 'Species': sub_sp.strip()})
        else:
            # Add as a single entry
            processed_entries.append({'Genus': genus, 'Species': species_part.strip()})

        # --- Add to final list based on conditions ---
        if is_exotic:
            # Always include exotic birds, they don't count towards the 400 limit
            bird_names.extend(processed_entries)
        elif valid_bird_count < max_valid_birds:
            # Add to valid birds if limit not reached
            for entry in processed_entries:
                if valid_bird_count < max_valid_birds:
                    bird_names.append(entry)
                    valid_bird_count += 1
                else:
                    # If adding multiple sub-species pushes past limit, stop for this entry
                    break
        # If not exotic and valid_bird_count >= max_valid_birds, the bird is implicitly skipped

    return bird_names

def write_to_csv(data, output_csv_path):
    """
    Writes a list of dictionaries to a CSV file.
    """
    if not data:
        print("No data to write to CSV.")
        return

    with open(output_csv_path, 'w', newline='', encoding='utf-8') as csvfile:
        fieldnames = ['Genus', 'Species']
        writer = csv.DictWriter(csvfile, fieldnames=fieldnames)

        writer.writeheader()
        for row in data:
            writer.writerow(row)
    print(f"Successfully wrote data to '{output_csv_path}'")

if __name__ == "__main__":
    html_file_name = 'page.html'
    output_csv_file = 'czech_bird_names_custom_filtered.csv' # New, distinct filename

    # Read the HTML content from the file
    try:
        with open(html_file_name, 'r', encoding='utf-8') as f:
            html_content = f.read()
    except FileNotFoundError:
        print(f"Error: The file '{html_file_name}' was not found.")
        exit()
    except Exception as e:
        print(f"An error occurred while reading the file: {e}")
        exit()

    # Extract bird names using the refined logic
    names = extract_czech_bird_names_from_html_refined(html_content)

    # Write the extracted names to a CSV file
    write_to_csv(names, output_csv_file)
