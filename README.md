# SimpleJournal
![GitHub](https://img.shields.io/github/license/andyld97/SimpleJournal)
![GitHub issues](https://img.shields.io/github/issues/andyld97/SimpleJournal)
![GitHub all releases](https://img.shields.io/github/downloads/andyld97/SimpleJournal/total)
![GitHub last commit (branch)](https://img.shields.io/github/last-commit/andyld97/SimpleJournal/dev)
![GitHub commit activity](https://img.shields.io/github/commit-activity/m/andyld97/SimpleJournal)
![GitHub Release Date](https://img.shields.io/github/release-date/andyld97/SimpleJournal)
![GitHub release (release name instead of tag name)](https://img.shields.io/github/v/release/andyld97/SimpleJournal?include_prereleases)
![Website](https://img.shields.io/website?down_color=lightgrey&down_message=offline&up_color=blue&up_message=online&url=https%3A%2F%2Fsimplejournal.ca-soft.net)

## Welcome

Welcome to the offical GitHub-Repo of SimpleJournal. SimpleJournal is a simple tool similar to OneNote or Windows Journal. The idea came from my best friend (Daniel S.) in 2018 ([see more details](https://simplejournal.ca-soft.net/en/about)) and since then SimpleJournal has evolved to a useful App which is also available in the Micorosft Store!

<a href='https://www.microsoft.com/en-US/p/simplejournal/9mv6j44m90n7?activetab=pivot:overviewtab'><img src='https://get.microsoft.com/images/en-us dark.svg' alt='English badge' width="150" /></a>

## Screenshot
![Screenshot](https://github.com/andyld97/SimpleJournal/blob/dev/Assets/screenshot.png "SimpleJournal App")

## Info

There are two versions of **SimpleJournal**, mainly due to compatibility requirements (for example, support for older Windows versions).

- **Normal version** – runs on Windows 7 and later  
- **Store version** – distributed via the Microsoft Store  

If you want to download the non-Store version, [click here](https://simplejournal.ca-soft.net/en/download).

Because of Microsoft Store restrictions, the two versions differ in functionality. The **Normal version** supports more features, which is why there are two distinct builds: `Normal` and `UWP`.

Although SimpleJournal is written in **WPF** (`.NET 10`), the Store version is **not** a native UWP application. It is packaged using the **Desktop Bridge** via the **MSIX Packaging Tool**.

### Features & Version Matrix

| Feature           | Normal Version     | Store Version      |
|-------------------|--------------------|--------------------|
| Paper Format | A4 | A4            |
| Page Pattern (*1) | Chequered, Dotted, Ruled, Blanco | Chequered, Dotted, Ruled, Blanco |
| PDF Support (*2)       | :heavy_check_mark: | :heavy_check_mark:  |
| Automatic Updates | :x:                | :heavy_check_mark: |
| Disable Touch     | :heavy_check_mark: | :x:                |
| Backup & Auto Save   | :heavy_check_mark: | :heavy_check_mark:                |
| Text & Form Recognition  | :heavy_check_mark: | :heavy_check_mark:                |
| Custom drawing tools  | :heavy_check_mark: | :heavy_check_mark:                |

- (*1) Since `v0.5.8.0` each page pattern can be edited and the offset can be set in cm (e.g. `0.5cm` for chequered)
- (*2) For PDF Support (since `v05.0.2`) it is required to install `Ghostscript` (see `PDF Support`)

## What is `Disable Touch`
This is a small solution that makes SimpleJournal more usable on devices that support touch input (not only pen input). A page in SimpleJournal is based on the WPF `InkCanvas`, which does not differentiate between input sources. Whether you write using a mouse, touch, or pen, the control treats all inputs the same. I was not able to reliably distinguish the input source within WPF’s `InkCanvas` (the newer `InkCanvas` in the UWP framework can do this).

The problem occurs when you write with a pen while resting your hand on a touch screen: both inputs are detected and rendered, resulting in unwanted strokes.

To prevent this, I implemented a solution that completely disables touch input using `devcon`. This works the same way as disabling a device via the Windows Device Manager. The feature is available as a settings option and is disabled by default. When enabled, the touch screen is deactivated on SimpleJournal startup and automatically reactivated when the last running instance of SimpleJournal is closed.

For the non-Store version, I also provide a small [tool](https://simplejournal.ca-soft.net/download.php?tdm=1) that allows you to manually enable or disable the touch screen. This feature is only integrated into the non-Store version because it requires administrator privileges, and currently there is no supported way to request elevated privileges from a Microsoft Store app.


## PDF Support
### Requirements
Since Simplejournal uses `Magick.NET` for processing PDF-files it is required to install [Ghostscript](https://ghostscript.com/releases/gsdnld.html).

### How does it work
In order to support pdf files SimpleJournal creates a journal out of the given pdf file that you can use in the app. The original pdf document is not affected in any ways! To convert a pdf document, SimpleJournal or the `PDF2J`-API creates a series of images from this document and zip it into the journal file.

### PDF2J - PDF to Journal
Converting large pdf files takes some time and require much computing power, so for low-end systems there is `PDF2J`. It's an ASP.NET-Core project with a little ticket managment system and the ability to convert the pdf file to a compatible journal. Of course it's also integrated into SimpleJournal (default host: http://cas-server2.ddns.net:8080) so you can also use this API. Don't worry after it's finished converting your document, your document (ticket) will be deleted! But anyways for advanced users there is also a possiblity to host this api on your own on Windows or Linux! (but remember that Ghostscript must be installed on that server too!)

### Limit: 100 Pages per document
Either using SimpleJournal itself or the converter api: There is a limit of 100 pages per document. For large documents (`> 100 pages`) multiple journals will be created. This limition is to reduce the amount of memory the programm uses when displaying large documents.
To simplify the workflow regarding large PDF-documents, you can navigate between these documents. At the top you can load the previous document (if available) and at the bottom you can load the next document (if available). This can also be disabeld in the settings dialog.

## Build
In order to work with form or text-recognition you need to compile `Analyzer` and then all files should be copied automatically while publishing!

## Thanks to
- Daniel S. for the great ideas and testing!
- Stefan E. for the great ideas and testing!
- Elmo for the dutch translation!