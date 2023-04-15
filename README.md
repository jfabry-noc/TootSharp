# TootSharp

## Motivation

This is a TUI (Terminal User Interface) client for interactive with Mastodon, likely similar to the [one I created for Tumblr](https://github.com/jfabry-noc/GoTumble) but hopefully a bit more useful since content on Mastodon is primarly text-based. The idea is to be able to easily use it from an SSH session and still achieve a usable experience. [tootstream](https://github.com/magicalraccoon/tootstream) is an inspiration that I use regularly.

## Goals

### Definite Goals

- Posting new text-based content. ✅
  - Include replies. ✅
  - Include CWs. ✅
  - Include polls.
  - Allow for re-editing of content if toot is too long.
- Viewing home, local, and federated timelines with caching. ✅
- Favoriting, boosting, and bookmarking content. ✅
- Viewing notifications.
- Allow for participation in polls from other users.

### Potential Goals

- Supporting image uploads.
- Following/unfollowing accounts. ✅
- Updating profile information.
- Viewing threads.

## Project State

Getting there! 💜

## Third Party Libraries

This project uses [AngleSharp](https://github.com/AngleSharp/AngleSharp) for parsing the HTML content of toots.
