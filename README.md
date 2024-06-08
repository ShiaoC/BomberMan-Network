# BomberMan-Network README

## English Version

### Introduction

This project is a networked version of the classic BomberMan game. It consists of a main Unity system for the game and two server scripts: `Server.py` for the main game's TCP server and `TimeServer.py` for the time synchronization's UDP server. Please run these server scripts before starting the game.

### How to Play

1. **Start the Servers**:
   - Run `Server.py` to start the TCP server for the main game.
   - Run `TimeServer.py` to start the UDP server for time synchronization.

2. **Connect to the Game**:
   - Launch the game.
   - On the initial connection page, enter the IP and port of the `Server.py` server.
   - Connect to the server.

3. **Start the Game**:
   - Wait for all players to connect.
   - Once the number of connected players equals the number of ready players, the game will start.
   - Press the ready button after everyone is connected.

4. **Game Controls**:
   - Use the WASD keys to move your character.
   - Press the spacebar to place bombs.
   - Power-ups such as increased bomb power, speed, and bomb quantity can be randomly obtained from the exploded blocks.

5. **Winning the Game**:
   - The last player standing becomes the Bomber King.

## 中文版

### 介紹

這個專案是一個經典遊戲BomberMan的網絡版本。它由主要的Unity系統和兩個伺服器腳本組成：`Server.py`用於主遊戲的TCP伺服器，`TimeServer.py`用於時間同步的UDP伺服器。請在開始遊戲之前先運行這些伺服器腳本。

### 遊戲說明

1. **啟動伺服器**:
   - 運行 `Server.py` 來啟動主遊戲的TCP伺服器。
   - 運行 `TimeServer.py` 來啟動時間同步的UDP伺服器。

2. **連接遊戲**:
   - 啟動遊戲。
   - 在初始的連接頁面中，輸入 `Server.py` 伺服器的IP和端口。
   - 連接伺服器。

3. **開始遊戲**:
   - 等待所有玩家連接。
   - 當已連接玩家數等於已準備玩家數時，遊戲將開始。
   - 在所有人連接後按下準備按鈕。

4. **遊戲控制**:
   - 使用WASD鍵移動角色。
   - 按下空白鍵放置炸彈。
   - 可以從爆炸後的方塊中隨機獲得炸彈威力、移動速度、炸彈數量等增強道具。

5. **贏得遊戲**:
   - 最後存活的人成為爆爆王。
