# SJFileAssoc
This project is required for the `UWP-Version` of SimpleJournal.
It is used to register *.journal-files to SimpleJournal.
`SJFileAssoc` is implemented in SimpleJournal as a `resource`.

## Reasons
In the store version SJ has no admin rights (e.g. for disabling the touchscreen),
because they won't be granted by Microsoft. But admin rights are not required for registering
for file extensions. 
But (and that's the reason why) the App is executed in a virtual environment which means
we don't have access to systems registry or I don't know any other way at the moment.
So SJ calls this program and this program registers the extension with the alias.
The alias is required because we have no real path of this application executed in the virtual
environment.

## Manifest

See `<uap5:ExecutionAlias Alias="SimpleJournal.exe" />`