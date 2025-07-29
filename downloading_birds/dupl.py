import csv
from collections import defaultdict

def find_duplicate_rows_in_csv(csv_file_path):
    """
    Finds duplicate rows in a CSV file and reports their line numbers.

    Args:
        csv_file_path (str): The path to the CSV file.
    """
    # defaultdict will automatically create a new list for a key if it doesn't exist
    row_occurrences = defaultdict(list)

    try:
        with open(csv_file_path, 'r', newline='', encoding='utf-8') as csvfile:
            reader = csv.reader(csvfile)

            # Read header first (assuming the first row is a header)
            header = next(reader, None)
            if header:
                print(f"Processing CSV with header: {', '.join(header)}")

            # Iterate through the rest of the rows
            for line_num, row in enumerate(reader, start=2): # Start from 2 because header is line 1
                # Convert the row list to a tuple so it can be used as a dictionary key
                row_tuple = tuple(row)
                row_occurrences[row_tuple].append(line_num)

        found_duplicates = False
        print("\n--- Duplicate Rows Found ---")
        for row_content, line_numbers in row_occurrences.items():
            if len(line_numbers) > 1:
                found_duplicates = True
                print(f"Row: {', '.join(row_content)}")
                print(f"  Appears on lines: {', '.join(map(str, line_numbers))}")
                print("-" * 30)

        if not found_duplicates:
            print("No duplicate rows found.")

    except FileNotFoundError:
        print(f"Error: The file '{csv_file_path}' was not found.")
    except Exception as e:
        print(f"An error occurred: {e}")

if __name__ == "__main__":
    # Replace 'your_file.csv' with the actual name of your CSV file
    csv_file = 'czech_bird_names_custom_filtered.csv'
    find_duplicate_rows_in_csv(csv_file)
