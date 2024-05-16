# python3 syncFetchedConsolidatedBlockFilesToProd.py
import os
import shutil

# Source directory containing .nyzoblock files
source_dir = "/var/www/blocks"

# Destination directory where folders will be created or files copied
destination_dir = "/var/lib/nyzo/production/blocks"

# Iterate over each file in the source directory
for filename in os.listdir(source_dir):
    if filename.endswith(".nyzoblock"):
        # Extract the folder name from the first 3 characters of the filename
        folder_name = filename[:3]
        # Construct the full destination path
        destination_path = os.path.join(destination_dir, folder_name)

        # Check if the destination folder exists, create it if not
        if not os.path.exists(destination_path):
            os.makedirs(destination_path)

        file_path = os.path.join(destination_path, filename)

	# Check if the file exists in the destination folder, copy it if it doesn't
        if not os.path.exists(file_path):
            shutil.copy(os.path.join(source_dir, filename), destination_path)