# SimpleJournal
![Lines Of Code](https://tokei.rs/b1/github/andyld97/SimpleJournal)
![GitHub](https://img.shields.io/github/license/andyld97/SimpleJournal)
![GitHub issues](https://img.shields.io/github/issues/andyld97/SimpleJournal)
![GitHub all releases](https://img.shields.io/github/downloads/andyld97/SimpleJournal/total)
![GitHub last commit (branch)](https://img.shields.io/github/last-commit/andyld97/SimpleJournal/dev)
![GitHub commit activity](https://img.shields.io/github/commit-activity/m/andyld97/SimpleJournal)
![GitHub Release Date](https://img.shields.io/github/release-date/andyld97/SimpleJournal)
![GitHub release (release name instead of tag name)](https://img.shields.io/github/v/release/andyld97/SimpleJournal?include_prereleases)
![Website](https://img.shields.io/website?down_color=lightgrey&down_message=offline&up_color=blue&up_message=online&url=https%3A%2F%2Fsimplejournal.ca-soft.net)

## Welcome

Welcome to the offical GitHub-Repo of SimpleJournal. SimpleJournal is a simple tool similar to OneNote or Windows Journal. The idea came from my best friend (Daniel S.) a few years ago ([see more details](https://simplejournal.ca-soft.net/en/about)) and since then SimpleJournal has evolved to a useful App which is also available in the Micorosft Store!

<a href='https://www.microsoft.com/en-US/p/simplejournal/9mv6j44m90n7?activetab=pivot:overviewtab'><img src='https://get.microsoft.com/images/en-us dark.svg' alt='English badge' width="150" /></a>

## Screenshot
![Screenshot](https://github.com/andyld97/SimpleJournal/blob/dev/Assets/screenshot.png "SimpleJournal App")

## Info

There are two versions of SimpleJournal due to compability issues, e.g. like supporting older versions of Windows. So we have the `normal version` which is running also on Windows 7 and the `store version`. If you want to download the non-store version [click here.](https://simplejournal.ca-soft.net/en/download)
Due to store-restrictions both versions are different: The normal version supports more features than the other, so therefore there are different builds: `UWP` and `Normal`. As you might have noticed that SimpleJournal is written in `WPF` (`.NET 8`), the Store-Version is not a real UWP-App. It is converted with the `Desktop Brdige` (`MSIX Packaging Tool`)

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
This is a small solution making SimpleJournal more usable on devices which support touch input (not only pen input). A page in SimpleJournal is based on the WPF `InkCanvas` which doesn't makes a difference according to the input source, so if you'll write via mouse, touch or pen this control cannot distinguish which input you have used respectively I wasn't able to differentiate it in the control (the newer `InkCanvas` from UWP Framework can do this). The problem is if you are writing with a pen while you put down your hand on your touch screen, both inputs are recognized and drawn and that leads to annoying results.
To prevent this I came up with a soultion which completely disables your touch screen based on `devcon`. This works the same way as your device manager do, as if you click on `Disable Device`. This is implemented as an option in the settings and it's not activated in the default settings. To enhance the usability your touch screen will be deactivated on the startup of SimpleJournal and will be reactivated if you close the last instance of SimpleJournal.

For the non-store version I created a simple [tool](https://simplejournal.ca-soft.net/download.php?tdm=1) which you can use for en/disabling your touch screen.
The reason why this feature is only integrated in the non-store version is, that this feature requires administrator privileges and currently I don't know how to aquire administrator privileges in the Store app!

## PDF Support
### Requirements
Since Simplejournal uses `Magick.NET` for processing PDF-files it is required to install [Ghostscript](https://ghostscript.com/releases/gsdnld.html).

### How does it work
In order to support pdf files SimpleJournal creates a journal out of the given pdf file that you can use in the app. The original pdf document is not affected in any ways! To convert a pdf document, SimpleJournal or the `PDF2J`-API creates a series of images from this document and zip it into the journal file.

### PDF2J - PDF to Journal
Converting large pdf files takes some time and require much computing power, so for low-end systems there is `PDF2J`. It's an ASP.NET-Core project with a little ticket managment system and the ability to convert the pdf file to a compatible journal. Of course it's also integrated into SimpleJournal (default host: http://cas-server2.ddns.net:8080) so you can also use this API. Don't worry after it's finished converting your document, your document (ticket) will be deleted! But anyways for advanced users there is also a possiblity to host this api on your own on Windows or Linux! (but remember that Ghostscript must be installed on that server too!)

### Limit: 100 Pages per document
Either using SimpleJournal itself or the converter api there is a limit of 100 pages per document. For large documents (`> 100 pages`) multiple journals gets created. This limition is to reduce the amount of memory the programm uses when displaying large documents.
A feature to "concat" the document (load the next document at the end of the current document or load the previous document at the end of the current document) is already planned to simplify the workflow using PDF journals.

## Build
In order to work with form or text-recognition you need to compile `Analyzer` and then all files should be copied automatically while publishing!

## Thanks to
- Daniel S. for the great ideas and testing!
- Stefan E. for the great ideas and testing!
- Elmo for the dutch translation!
