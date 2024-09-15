# OwnTracksRecUtils

This is a supplementary tool for my [Vaultracks](https://github.com/Metal-666/Vaultracks) project. Its primary purpose is to help with data migration from existing .rec files to an SQLite DB. <sub><sup><sub>It also has the ugliest UI I have ever created.</sub></sup></sub>

### Important

Similarly to Vaultracks, this app only handles `"location"` data. Other stored messages, like `"waypoint"` or `"transition"` will be ignored.

## Migration steps

- Download, unzip and launch the [latest release](https://github.com/Metal-666/Vaultracks/releases/latest).
- If you've used [OwnTracks Recorder](https://github.com/owntracks/recorder) before, you might have several .rec files. You can use the `Merge .rec files` button in my app to combine them into a single .rec file.
- Now lets convert it into a format that can be imported into a SQLite database - CSV. If you like to tinker with command line tools, you can use an official OwnTracks utility - [ocat](https://github.com/owntracks/recorder?tab=readme-ov-file#ocat). Alternatively, use the `.rec to .csv` button in my app.
- Keep an eye on the Status field at the bottom. If something goes wrong during previous steps, the relevant info will be printed there.
- Now that we have our .csv file, lets import it in the database. For this you will need an app for managing SQLite databases. I personally use [SQLiteStudio](https://github.com/pawelsalawa/sqlitestudio). If you are using an encrypted database (created, for example, with Vaultracks), make sure the app supports SQLCipher. Another key feature that needs to be supported is the automatic creation of primary keys (since the exported .csv file has an empty Id column). I guess you could also manually fill the Id column with indexes?...
- Use the chosen DB management app to open your database file.
- Locate the Location table and find the option to import data from a .csv file.
- Make sure the option to treat the first line as column names is checked. If you are using SQLiteStudio, also make sure to check the `NULL values` option and leave the text box empty.
- Import the data and verify that the everything is there. A quick way to do this is to check how many rows where added: the number should be equal to the number of rows in your .csv file minus 2 (the header row and the empty line at the end).
