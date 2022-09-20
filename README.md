# LinkdingTools

This is a collection of tools for [linkding](https://github.com/sissbruecker/linkding). The project is customized for my
own needs and offers the following functions.

- Import of a bookmark export from browsers (tested with chrome and edge)
- In addition, there is another application that enriches bookmarked links with tags.

## Requirements

The applications are available as source code. In order to use these applications, it is best to create them locally.
For this purpose the repository can be cloned here.

Furthermore, a running linkding instance is required. In the target linkding instance, an access token should also be
created ([documented here](https://github.com/sissbruecker/linkding/blob/master/docs/API.md#authentication)).

The URL of the linkding instance and the key must be available to the applications.

## Linkding.Importer

The importer imports bookmarks exported as html from the browsers.
The bookmarks from Chrome can be exported via the interface.
The bookmark manager can be accessed via the ctrl+shift+o key combination, then the item "Export bookmarks" can be
selected in the context menu in the upper right corner.

The complete folder structure is assigned to the bookmark as a tag. Additionally, year of the date where the bookmark
was added is stored as a tag.

## Linkding.Updater

The updater is intended to scan all bookmarks in certain intervals and make updates to the bookmarks based on the
handlers. This application can be controlled e.g. with crontab on a Linux system or via the default scheduler on
Windows.

Currently, there are 2 handlers. One set at a new bookmark the year as tag (the year from the date where the bookmark
linkding was added). The other handler sets tags based on defined rules. As an example, a link to a subreddit in reddit
is set with the tags reddit and the subreddit name.