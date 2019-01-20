---
layout: single
title:  "How hard could it be to make a multi host Postgres proxy?"
date:   2019-01-20 15:30:00 -0500
category: tech
---

Imagine a scenario where you want to route postgres connections through a single proxy to multiple databases.
```
db1.com ---> Proxy ---> Database 1
db2.com --->   |   ---> Database 2
```
How would the proxy work? We'd want to leave the actual database connection secured so the data itself would be encrypted. That prevents us from proxying based off some data in the query itself. That leaves us with a couple of options
1. Connecting IP Address
1. Hostname

Option 1 is simple enough, but it would prevent the same user in most scenarios from being able to access both databases. Ideally depending on the hostname the user connected to the proxy could pick up on that and route accordingly! Unfortunately that's just not possible.

But HTTP connections do that all the time with reverse proxies like nginx! What gives?

HTTP can rely on the host header. Even end-to-end HTTPS connections can support this model with [Server Name Indication](https://en.wikipedia.org/wiki/Server_Name_Indication) (although unlike the rest of the traffic, the hostname is sent unencrypted)

At the end of the day, the hostname is just not a concept that exists for general TCP connections. That's why DNS exists, to transform the hostname into an IP address before it ever gets routed to the server. Unfortunately as of writing, it doesn't seem like Postgres supports SNI (The only reference I could find to it was a [suggestion in a mailing list](https://www.postgresql.org/message-id/CAPPwrB_tsOw8MtVaA_DFyOFRY2ohNdvMnLoA_JRr3yB67Rggmg%40mail.gmail.com))

So at the moment, if you truly want only one proxy to handle connections you'll have to rely on IP addresses. Otherwise you'll need 1 proxy for every database you wish to connect to.
```
db1.com ---> Proxy 1 ---> Database 1
db2.com ---> Proxy 2 ---> Database 2
```