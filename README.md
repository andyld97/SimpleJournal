# SimpleJournal

## Welcome

Welcome to the offical GitHub-Repo of SimpleJournal. SimpleJournal is a simple tool similar to OneNote or Windows Journal. The idea came from my best friend (Daniel S.) a few years ago ([see more details](https://simplejournal.ca-soft.net/en/index.php?page=about)) and since then SimpleJournal has evolved to a useful App which is also available in the Micorosft Store!

<a href='https://www.microsoft.com/en-US/p/simplejournal/9mv6j44m90n7?activetab=pivot:overviewtab'><img src='https://developer.microsoft.com/store/badges/images/English_get-it-from-MS.png' alt='English badge' width="150" /></a>

There are two versions of SimpleJournal due to compability issues, e.g. like supporting older versions of Windows. So we have the `normal version` which is running also on Windows 7 and the `store version`. If you want to download the non-store version [click here.](https://simplejournal.ca-soft.net/en/index.php?page=downloads)
Due to store-restrictions both versions are different: The normal version supports more features than the other, so therefore there are different builds: `UWP` and `Nornal`. As you might have noticed that SimpleJournal is written in `WPF` (`.NET 4.8`), the Store-Version is not a real UWP-App. It is converted with the `Desktop Brdige` (`MSIX Packaging Tool`)

### Features & Version Differences

| Feature           | Normal Version     | Store Version      |
|-------------------|--------------------|--------------------|
| Paper Format | A4 | A4            |
| Automatic Updates | :x:                | :heavy_check_mark: |
| Disable Touch     | :heavy_check_mark: | :x:                |
| Backup & Auto Save   | :heavy_check_mark: | :heavy_check_mark:                |
| Checkered, lined and blank pages   | :heavy_check_mark: | :heavy_check_mark:                |
| Text & Form Recognition  | :heavy_check_mark: | :heavy_check_mark:                |
| Custom drawing tools  | :heavy_check_mark: | :heavy_check_mark:                |

## What is "Disable Touch"
This is a small solution making SimpleJournal more usable on devices which support touch input (not only pen input). A page in SimpleJournal is based on the WPF `InkCanvas` which doesn't makes a difference according to the input source, so if you'll write via mouse, touch or pen this control cannot distinguish which input you have used respectively I wasn't able to differentiate it in the control (the newer `InkCanvas` from UWP Framework can do this). The problem is if you are writing with a pen while you put down your hand on your touch screen, both inputs are recognized and drawn and that leads to annoying results.
To prevent this I came up with a soultion which completely disables your touch screen based on `devcon`. This works the same way as your hardware manager do, as if you click on `Disable Device`. This is implemented as an option in the settings and it's not activated in the default settings. To enhance the usability your touch screen will be deactivated on the startup of SimpleJournal and will be reactivated if you close the last instance of SimpleJournal.

For the non-store version I created a simple [tool](https://code-a-software.net/simplejournal/setups/tdm.zip) which you can use for en/disabling your touch screen.

## Thanks to
- Daniel S. for the great ideas and testing
- Stefan E. for the great ideas and testing
