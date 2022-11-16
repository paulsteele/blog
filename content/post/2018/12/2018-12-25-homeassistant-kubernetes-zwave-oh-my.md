---
categories:
- tech
date: "2018-12-25T17:00:01Z"
title: Home Assistant, and Kubernetes, and Z-Wave, Oh My!
---
### Home Assistant + Z-Wave
For the last 3 years or so, I've been running [Home Assistant](https://www.home-assistant.io/) for my home automation needs. I've gone through a couple iterations with it being hosted on bare metal on a raspberry pi, in a python virtual environment, and within a docker container. I started off mainly with just smart lights so wi-fi bulbs worked pretty well. After time, concerns with security and general scale led me to start using [Z-Wave](https://www.z-wave.com/) and [Zigbee](https://www.zigbee.org/) devices. These required physical hardware to hook up to the raspberry pi (After switching around a bit I currently use the [Nortek HUSBZB-1](https://www.nortekcontrol.com/products/2gig/husbzb-1-gocontrol-quickstick-combo/)).

Within Home Assistant, this requires linking to the `tty` device like so
```
zwave:
  usb_path: /dev/ttyUSB0
zha:
  usb_path: /dev/ttyUSB1
```
On bare metal, and a virtual python environment, this is dead simple. Docker has an extra step but it isn't that complicated. When starting the container you just need to add an extra `--device` flag like so.
```
docker run --device=/dev/ttyUSB0:/dev/ttyUSB0 --device=/dev/ttyUSB1:/dev/ttyUSB1 homeassistant/home-assistant
``` 
Everything was working great and I could control my devices. So I should have probably stopped there, but of course I couldn't just leave well enough alone.

### Enter Kubernetes

Around a year ago I upgraded to an Intel NUC as I wanted to host more containers for various other services (this blog for example). As part of that I wanted to toy around with orchestrating everything with [Kubernetes](https://kubernetes.io/). I had some experience with it at work and even though a single node, bare metal environment isn't what it's suited for, I still wanted to give it a whirl.

_Cue several weekends of banging my head against Kubernetes documentation_

After a couple weeks I had most of what I wanted set up. All of the new services that I wanted running were behaving satisfactorily. I had one issue though, Home Assistant, the first service that started me down the path of running containers, wasn't playing so nicely with Kubernetes. The main problem was there is no direct equivalent to the `--device` flag (See a related [GitHub Issue](https://github.com/kubernetes/kubernetes/issues/5607)). There was a workaround that did make everything seem alright. On the pod container configuration, under `securityContext` if `privileged` was set to `true` the container could mount devices successfully.
```
containers:
  - name: homeassistant
    ...
    volumeMounts:
    - mountPath: /dev/ttyUSB
      name: dev-usb0
    securityContext:
      privileged: true
volumes:
  - name: dev-usb0
    hostPath:
      path: /dev/ttyUSB0
```
However making a container privileged comes with a whole slew of other consequences besides being able to mount devices like [allowing root users running in the container essentially to act like root users on the host machine](https://kubesec.io/basics/containers-securitycontext-privileged-true/) (As of writing, [Home Assistant does run as root within its container](https://github.com/home-assistant/home-assistant/issues/7872)). This left a sour taste in my mouth. I didn't like running the container with such privileges, but there wasn't an alternative that I could find at the time so I dealt with it.

### Enter ser2net & socat

Several months later, I stumbled upon a [post](https://community.openhab.org/t/share-z-wave-dongle-over-ip-usb-over-ip-using-ser2net-socat-guide/34895) talking about two programs [`ser2net`](https://linux.die.net/man/8/ser2net) and [`socat`](https://linux.die.net/man/1/socat). In a grossly oversimplified explanation. `ser2net` will expose devices (like the Z-Wave / Zigbee usb stick) over a TCP connection, while `socat` can consume a TCP connection and "recreate" the device on a different machine. This combination would allow the usb stick to be mounted on an entirely different machine, but for my purposes `ser2net` will be run on the host machine, while `socat` will be run inside the container.

#### Setting up ser2net
After installing `ser2net`, modifying `/etc/ser2net.conf` was needed to expose the usb device. The Nortek HUSBZB-1, while being one physical USB stick, actually exposes two `tty` devices (`/dev/ttyUSB0` for Z-Wave, and `dev/ttyUSB1` for Zigbee) so both will need to be exposed.
```
3333:raw:0:/dev/ttyUSB0:115200 8DATABITS NONE 1STOPBIT
3334:raw:0:/dev/ttyUSB1:57600 8DATABITS NONE 1STOPBIT
```
Essentially each line exposes one device, the first one on port 3333, the second one on port `3334`. As an aside, I was only able to get this setup to work with having the devices at different baud rates.

#### Setting up socat
Initially, I had wanted to use a separate container in the home assistant pod but due to how socat handles the user defined location of the device file (a symbolic link to `/dev/pts`), sharing the mounted device between containers was a hurdle I couldn't overcome. This left the option of running `socat` within the homeassistant container itself. Ideally this would probably be a custom built docker image with additional binaries installed and a custom entrypoint, but I enjoy the frequent updates that home assistant provides and didn't want to maintain a new image. This left installing, and running `socat` from the kubernetes configuration! Firstly, I added the required `init.d` scripts to get socat running on boot in a `configmap`.
```
apiVersion: v1
kind: ConfigMap
metadata:
  name: homeassistant-systemd-services
data:
  zwave: |
    #!/bin/bash
    case "$1" in 
        start)
          socat pty,link=/dev/ttyUSB0,raw,user=0,group=0,mode=777 tcp:[NODE_IP]:3333 &
          ;;
        stop)
          stop
          ;;
        restart)
          stop
          start
          ;;
        status)
          ;;
        *)
          echo "Usage: $0 {start|stop|status|restart}"
    esac
    exit 0 
  zha: |
    #!/bin/bash
    case "$1" in 
        start)
          socat pty,link=/dev/ttyUSB1,raw,user=0,group=0,mode=777 tcp:[NODE_IP]:3334 &
          ;;
        stop)
          stop
          ;;
        restart)
          stop
          start
          ;;
        status)
          ;;
        *)
          echo "Usage: $0 {start|stop|status|restart}"
    esac
    exit 0 
```
The important part of the script being:
```
socat pty,link=/dev/ttyUSB0,raw,user=0,group=0,mode=777 tcp:[NODE_IP]:3333
```
which attempts to open a connection to the node at port `3333`(where `[NODE_IP]` is the actual ip address of the device running `ser2net`).

The configmap then had to be mounted as a volume so the container could see it. (I had to manually change the permissions of the file so it could be used)
```
volumes:
  - name: zwave-config
    configMap:
    name: homeassistant-systemd-services
    defaultMode: 0755
    items: 
        - key: zwave
          path: zwave
  - name: zha-config
    configMap:
    defaultMode: 0755
    name: homeassistant-systemd-services
    items: 
        - key: zha
          path: zha
```

Then `socat` needed to be installed when the container started. Additionally the two services needed to be run. This was done modifying the container's `command` and `args` properties.
```
spec:
  containers:
    - name: homeassistant
        image: homeassistant/home-assistant
        ports: 
          - containerPort: 8123
          - containerPort: 8300
        command: ["sh"]
        args: ["-c", "apt-get update && apt-get install socat && service zwave start && service zha start && python -m homeassistant --config /config"]
        volumeMounts:
          - name: config
            mountPath: "/config"
          - name: zwave-config
            mountPath: /etc/init.d/zwave
            subPath: zwave
          - name: zha-config
            mountPath: /etc/init.d/zha
            subPath: zha
```
Once all the configurations had been applied, home assistant picked up on the z-wave and zigbee devices and my lights were connected once more.


Not the most elegant solution, but [getting to remove that privileged configuration](https://github.com/paulsteele/eos-setup/commit/5d731fa2ece2c3c06136c066b6debd570a5c6ee1#diff-ea60c368e98c5d9dbdb4ded99eff6f1eL66) felt pretty good.
