# BookBase
NOTE: this is still a WIP 

BookBase is a cross-platform .NET MAUI personal library manager built with MVVM, SQLite, and dependency injection. It runs on Android, iOS, macOS Catalyst, and Windows.

## Features

### Library Management
- Add, edit, and delete books with rich metadata: title, subtitle, author, publisher, description, ISBN-10/13, format, language, page count, cover image, and publication date.
- Track physical ownership, wishlist status, and shelf location for each book.
- Mark books as a **Favorite**.
- Record the price paid for owned books.

### Reading Status & Progress
- Assign a reading status to every book: **Want to Read**, **Reading**, **Finished**, **Paused**, or **Did Not Finish**.
- Log your current page to track reading progress; the app automatically marks a book as **Reading** when you start and **Finished** when you reach the last page.
- Record start and finish dates automatically as you update progress.
- Each page-update creates a **Reading Session** entry so you can review how many pages you read in each sitting.

### Dashboard
- At-a-glance statistics: books currently being read, books finished this year, books owned, wishlist count, total pages read, and average rating.
- Carousels showing the **Newest Additions** and **Recently Finished** books.

### ISBN Scanner
- **Barcode scan mode** — point the camera at any EAN-13, UPC-A, EAN-8, or Code 128 barcode to instantly identify a book.
- **OCR mode** (Android) — photograph printed ISBN text and the app uses on-device ML Kit text recognition to extract and validate ISBN candidates. Select from the detected ISBNs to continue.
- Detected ISBNs are validated against the standard checksum before being accepted.

### Automatic Book Lookup
- When an ISBN is scanned or entered, BookBase queries **Open Library** and then **Google Books** in order to pre-populate title, author, publisher, description, page count, language, and cover art.
- Existing books in the local database are matched first so no unnecessary network calls are made.

### Ratings & Notes
- Rate books on a numeric scale.
- Save personal notes against any book.

### Import & Export
- **Export to JSON** — full book data for backup or transfer.
- **Export to CSV** — lightweight spreadsheet-friendly format (Id, ISBN13, Title, Author, Status, Rating).
- **Import from JSON** — re-import a previously exported file; duplicate ISBNs are skipped automatically.

### Backup & Restore
- Copy the SQLite database file to any location on the device.
- Restore a database backup to replace the current library.

### Settings
- Persistent user preferences managed through a dedicated settings service.

## Supported Platforms

| Platform | Minimum OS |
|---|---|
| Android | API 24 (Android 7.0) |
| iOS | 15.0 |
| macOS Catalyst | 15.0 |
| Windows | 10.0.17763 (1809) |

> OCR-powered ISBN text recognition is currently supported on **Android only** (via Google ML Kit). Other platforms fall back to barcode scanning and manual ISBN entry.

## Tech Stack

- **.NET 9 / .NET MAUI** — cross-platform UI framework
- **CommunityToolkit.Mvvm** — MVVM source generators and `ObservableObject`
- **CommunityToolkit.Maui** — additional MAUI helpers and converters
- **sqlite-net-pcl** — embedded SQLite database
- **ZXing.Net.Maui** — barcode scanning via device camera
- **Xamarin.Google.MLKit.TextRecognition** (Android) — on-device OCR
- **Open Library API** & **Google Books API** — online book metadata

## Build

```bash
dotnet build src/BookBase/BookBase.csproj -f net9.0-android
```

On macOS, iOS and macOS Catalyst targets are also available:

```bash
dotnet build src/BookBase/BookBase.csproj -f net9.0-ios
dotnet build src/BookBase/BookBase.csproj -f net9.0-maccatalyst
```

On Windows:

```bash
dotnet build src/BookBase/BookBase.csproj -f net9.0-windows10.0.19041.0
```

## Tests

```bash
dotnet test tests/BookBase.Tests/BookBase.Tests.csproj
```
