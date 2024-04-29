Test project for the @construct0/nyzo.cl npm package. The version in package.json indicates to which nyzo.cl version it has been weakly aligned with.




## Recommended test environment
- npm v10.5.2 and above
- node v21.7.3 and above
- OS
  - macOS 10.15 and above (Intel or Apple Silicon 64-bit (x64 or arm64))
  - Linux Ubuntu 20.04 and above, Fedora 38 and above, and Debian 10 and above (x64 or arm64)
  - Windows 10 and above (64-bit only)
- Specs
  - 2 cores and above
  - 6 GB ram and above
- Browser
  - Chrome and/or Firefox



## Setting up the test environment 
Instead of relying on package.json to manage the dependency and version matching, we manually link them together, this encourages contributors to immediately create and update tests for any contributions made to the @construct0/nyzo.cl package.

It is recommended to **not** run the following commands as root.
```
git clone https://github.com/construct0/nyzoResources.git
cd nyzoResources/JavaScript/NyzoCL
npm install
npm link
cd ../NyzoCL.Tests
npm install
npm link @construct0/nyzo.cl
```




**GNU/LINUX ONLY - installing cypress system dependencies**
```
# Ubuntu/Debian
apt-get install -y libgtk2.0-0 libgtk-3-0 libgbm-dev libnotify-dev libnss3 libxss1 libasound2 libxtst6 xauth xvfb

# Arch
pacman -S gtk2 gtk3 alsa-lib xorg-server-xvfb libxss nss libnotify

# CentOS
yum install -y xorg-x11-server-Xvfb gtk2-devel gtk3-devel libnotify-devel GConf2 nss libXScrnSaver alsa-lib

# Amazon Linux 2023
dnf install -y xorg-x11-server-Xvfb gtk3-devel nss alsa-lib

# Docker
# https://github.com/cypress-io/cypress-docker-images
```




## Running the tests
Running the following commands as root may result in unexpected errors. 

**Starting the HTTP server**
```
npx http-server -p 8080
```
Confirm the HTTP server is serving content from the *./public* folder by visiting
- http://localhost:8080

---
**Starting the cypress client & dedicated browser window**

In a **new terminal window** run the following command, do not close the other terminal window.
```
npm run test
```

The Cypress GUI client should open, continue by 
- selecting E2E testing
- selecting a browser of your choice
---
A new browser window should open, continue by
- navigating to the Specs page
- selecting a spec of your choice

*This browser window is dedicated to cypress and is managed and controlled by your local cypress client, keep this in mind when visiting other websites in the same window.*

---
The specs' tests will automatically run, inspect to your heart's desire using the browser developer tools and cypress client.




Do not edit the `./public/nyzo.cl.bundle.js` file, it is overwritten when running the `npm run test` command.




