# DemoApp.Messages

[MS Doc starting docker](https://learn.microsoft.com/en-us/dotnet/core/docker/build-container)
[podman Debian](https://gist.github.com/matinrco/5515e213f6aaadae47dc4af003805385)

## Wsl Debian 11
```
sudo apt update && sudo apt install wget lsb-release -y

source  /etc/os-release

sudo apt install gpg curl openssl -y
sudo apt update && sudo apt full-upgrade -y

# https://software.opensuse.org/download/package?package=podman&project=devel%3Akubic%3Alibcontainers%3Astable
echo 'deb http://download.opensuse.org/repositories/devel:/kubic:/libcontainers:/stable/Debian_Unstable/ /' | sudo tee /etc/apt/sources.list.d/devel:kubic:libcontainers:stable.list

curl -fsSL https://download.opensuse.org/repositories/devel:kubic:libcontainers:stable/Debian_Unstable/Release.key | gpg --dearmor | sudo tee /etc/apt/trusted.gpg.d/devel_kubic_libcontainers_stable.gpg > /dev/null

sudo apt update && sudo apt install podman -y

sudo update-alternatives --set iptables /usr/sbin/iptables-legacy

```

```
podman network create demoapp-network

mkdir -p .containers/queue/data .containers/queue/log

podman run -d --rm --name demoapp-messages-mq --network demoapp-network -p 5672:5672 -p 15672:15672  -e 'RABBITMQ_DEFAULT_USER=guest' -e 'RABBITMQ_DEFAULT_PASSWORD=guest' docker.io/library/rabbitmq:3-management
```

##
```
sudo apt update && sudo apt -y full-upgrade 
sudo apt install python3-pip podman 
```

## Host
`wsl --install Debian`

## Wsl
### Prerequisites
```
sudo apt update && sudo apt upgrade
sudo apt install openssl ca-certificates curl -y
sudo apt install podman -y
```

#### `sudo -e /etc/containers/containers.conf`
Add 
```
[containers]
pids_limit = 4096
[engine]
events_logger = "file"
```

### `sudo -e /etc/containers/registries.conf.d/shortnames.conf`
Add
```
[aliases]
  # nginx
  "nginx" = "docker.io/library/nginx"
```

`podman run --name nginx -p 8080:80 -d nginx:alpine`
`podman ps -a`

Check running by curl:
`curl localhost:8080`

### Cleanup
`podman stop nginx`
`podman rm nginx`

Possibly also `podman rmi nginx`.

## Host
Check localhost:8080 from host system using browser.

## Wsl
 
### `podman build -t demoapp-messages-image -f Dockerfile`

Output:
```sh
STEP 1: FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
Getting image source signatures
Copying blob d77e3284f2dc done
...
Copying config f33f8a781d done
Writing manifest to image destination
Storing signatures
STEP 2: WORKDIR /app
--> 182ffa09a8d
STEP 3: EXPOSE 80
--> 806933e01a6
STEP 4: EXPOSE 443
--> fc2638b5d34
STEP 5: FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
Getting image source signatures
Copying blob f03b40093957 skipped: already exists
...
Copying config 5c0f3335e8 done
Writing manifest to image destination
Storing signatures
STEP 6: WORKDIR /src
--> f0f6c5b5266
STEP 7: COPY ["DemoApp.Messages.csproj", "."]
--> 39d7ce191e8
STEP 8: RUN dotnet restore "DemoApp.Messages.csproj"
  Determining projects to restore...
  Restored /src/DemoApp.Messages.csproj (in 3.12 sec).
--> 558a467d0d7
STEP 9: COPY . .
--> 1e42f044e0a
STEP 10: WORKDIR "/src"
--> 9bb312454bd
STEP 11: RUN dotnet build "DemoApp.Messages.csproj" -c Release -o /app/build
MSBuild version 17.6.1+8ffc3fe3d for .NET
  Determining projects to restore...
  All projects are up-to-date for restore.
  DemoApp.Messages -> /app/build/DemoApp.Messages.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:03.07
--> 128cc82de59
STEP 12: FROM 128cc82de595bd42bba1fca2ecfff45a7abd84b3c7e96d5377d33471eaed0842 AS publish
STEP 13: RUN dotnet publish "DemoApp.Messages.csproj" -c Release -o /app/publish /p:UseAppHost=false
MSBuild version 17.6.1+8ffc3fe3d for .NET
  Determining projects to restore...
  All projects are up-to-date for restore.
  DemoApp.Messages -> /src/bin/Release/net7.0/DemoApp.Messages.dll
  DemoApp.Messages -> /app/publish/
--> c1b19e78bb2
STEP 14: FROM mcr.microsoft.com/dotnet/runtime:7.0
Getting image source signatures
Copying blob d77e3284f2dc skipped: already exists
...
Copying config 32953498d6 done
Writing manifest to image destination
Storing signatures
STEP 15: WORKDIR /app
--> b90bce1618e
STEP 16: COPY --from=publish /app/publish .
--> 537bd20e6a9
STEP 17: ENTRYPOINT ["dotnet", "DemoApp.Messages.API.dll"]
STEP 18: COMMIT demoapp-messages-image
--> 556beb1013f
556beb1013f70a0ab932370220706df9fdf99a415712b7954489e65fd94fa9a3
```

Verify it was created:
```sh
$ podman images
REPOSITORY                        TAG     IMAGE ID      CREATED             SIZE
localhost/demoapp-messages-image  latest  556beb1013f7  About a minute ago  200 MB
```

## Create the container
`podman create --name demoapp-messages-container demoapp-messages-image`

Verify it was created:
```sh
$ podman ps -a
CONTAINER ID  IMAGE                                    COMMAND  CREATED        STATUS   PORTS   NAMES
a1a7799a9111  localhost/demoapp-messages-image:latest           8 seconds ago  Created          demoapp-messages-container
```

Run it:
`podman start demoapp-messages-container`

Run it for debugging and manual start of the entrypoint:
`podman run -it --rm --entrypoint "bash" demoapp-messages-image`

`--rm` removes container after exiting.
`-i` for interactive.

For breaking the session "ctrl-p,ctrl-q" (--detach-keys [a-Z])

# docker compose

`sudo apt install python3 python3-pip -y`

Since debian is using an older version of podman (unless added the Suse repo or using Ubuntu version) we need an older version of podman-compose:
`pip3 install 'podman-compose<1.0'`

If `  WARNING: The script dotenv is installed in '/home/username/.local/bin' which is not on PATH.`:

Edit ~/.bashrc:
```
export PATH=$PATH:$HOME/.local/bin
```

`source ~/.bashrc`
