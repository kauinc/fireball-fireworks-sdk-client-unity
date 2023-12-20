# Changelog

## [0.9.0] - 2023-12-20
- re-organizing folder structure
- removed legacy messengers

## [0.8.5] - 2023-11-30
- added game variants support
- added get balance
- updated GCI events from operator and from game
- updated multiplayer broadcast messages listener

## [0.8.4] - 2023-09-15
- added free bets details
- added gci integration error event
- added error dialog and error client script on error response
- update currency in session and extra if it changed after authorization
- update web browser set url from iframe
- update error response with optional balance param
- update network checker and show connection error

## [0.8.3] - 2023-07-10
- added game client interface (gci) module
- added free bets campaign support
- added client vars parsing on any base response
- fixed exceptions logging

## [0.8.2] - 2023-06-12
- added multiplayer module
- added get bet tiers list
- added translations support
- add communicator module
- update demo authorize with nice params
- remove coins and last action id from auth response
- updated reconnection logic

## [0.8.1] - 2022-11-23
- added transaction replays
- added custom Action Id for Auth Request
- fix game url parsing
- fix thread dispatcher dispose
- fix fireball instance destroy

## [0.8.0] - 2022-10-05
- added new communication plugin Best HTTP/2 as main messanger

## [0.7.0] - 2022-09-21
- added transaction list history
- added log levels to logger
- updated Authorization and Deme Authorization
- updated SignalR reconnects process
- updated core urls
- fixed sitting fireball instance on awake

## [0.6.5] - 2022-07-13
- fixed "A Native Collection has not been disposed" error

## [0.6.4] - 2022-07-08
- added timestamp field to jackpots update

## [0.6.3] - 2022-06-21
- update SignalR to Unity 2021 and newer
- replace "Pointer_stringify" to "UTF8ToString"

## [0.6.2] - 2022-06-20
- updated "NativeWebSocket" to v1.1.4

## [0.5.1] - 2022-02-10
- This is the first release of Fireball SDK as package.
