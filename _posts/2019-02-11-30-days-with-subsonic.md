---
layout: single
title:  "30 Days with Subsonic"
date:   2019-02-11 19:50:00 -0500
category: life
---

So it's been a little over a month since I [ditched Plex]({{ site.baseurl }}{% link _posts/2019-01-12-before-and-after-plex.md %}) in favor of [Subsonic](http://www.subsonic.org/pages/index.jsp), and I've started to get the hang of things. I was initially a little skeptical if I would like the service enough to fork out for the [premium](http://www.subsonic.org/pages/premium.jsp) version or not. With a 30 day free trial I didn't have much to lose however. 

There wasn't an official docker image, unlike Plex, so I went with the one provided by [mbirth](https://hub.docker.com/r/mbirth/subsonic/) which works well enough. As an aside, Kubernetes and Docker made trying this out super simple. I left around my old Plex configs for a couple of days until I was satisfied that Subsonic would work. I was able to modify my volume mounts without having to move any of my actual media and Subsonic picked up on it right away.

## The Bad
So I'll start off with the things I don't necessarily like.
* Plex's User interface is certainly much prettier (Plex on the left, Subsonic on the right)

![plex-ui.jpeg](/assets/posts/2019-02-11-30/plex-ui.jpeg)
![subsonic-ui.jpeg](/assets/posts/2019-02-11-30/subsonic-ui.jpeg)

* The mobile app for Plex is much more usable. The default app for Subsonic was frankly a pain to interact with.
* Subsonic has some issues with file names containing non ASCII characters in them. (If an artist has them, their music won't show up in the interface at all. If a media file has them then they'll appear as folders in the interface. In either situation they are unplayable)

## The Good
However many of the flaws listed above have workarounds.
* Subsonic has a decent selection of built in themes that make the colors more pleasing to look at.
* [DSub](https://play.google.com/store/apps/details?id=github.daneren2005.dsub&hl=en_US) is a great third party mobile app for interacting with Subsonic. I highly recommend it over the official app.
* As mentioned in my previous post, Subsonic's home screen is much more pleasurable to use. The 'Random' tab is still my favorite. I can easily refresh it several times until I find something that I'm in the mood for.
* Adding items to the end of the music queue is much quicker in subsonic compared to plex.
* The killer feature for me is definitely the caching behavior in Subsonic. It is so much more flexible than in Plex. If I was planning on going on a long car ride with Plex, I would have to go through an additional step of making sure to download each album I wanted to play and wait for everything to finish and then separately add all the music to a playlist. Furthermore if I forgot to download music, Plex from what I could see, wouldn't preload songs. If my connection was slow, music would come grinding to a halt while the next song buffered. Subsonic (DSub) can be configured to preload music once it's added to a queue. There are separate settings for how many songs to preload depending on if it is connected to WiFi or mobile data. This works especially well for me since I'll queue up my playlist for the day at home. By the time I walk out the door, all the music is already downloaded and I end up not using any of my cellular data quota for the day. In the cases where I change my mind after leaving the house, the preload also ensures some buffer should there be a temporary gap in cellular coverage. 

## Conclusion
With my 30 day trial used up, I decided to keep using Subsonic over Plex. The downsides aren't nearly large enough for me to consider moving back to Plex. I'm sure over time I'll find other quirks with the application, but for the foreseeable future it seems to fit the bill.