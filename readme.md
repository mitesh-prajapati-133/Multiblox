# 🎮 MultiBlox

**MultiBlox** is a multiplayer card game developed with **Unity and C#**, featuring competitive card gameplay, bot-based matches, friend-based multiplayer, player profiles, authentication, social interactions, and real-time game synchronization.

The game supports **2, 3, and 4-player matches**, including matches against bots and multiplayer games with friends.

---

## 🚀 Key Features

### 🎴 Card Gameplay

* 52-card deck-based gameplay
* Cards distributed among players
* Players play cards one by one into the center stack
* Matching-card detection
* Center stack collection mechanism
* Card-count based winner detection
* Time-based match completion
* Automatic winner determination
* Support for 2, 3, and 4 players

### 🤖 Bot Mode

* One real player can play against bots
* Automated bot decision-making
* Supports multiplayer configurations
* Same core game rules as multiplayer mode

### 🌐 Multiplayer

* Create and join multiplayer lobbies
* Referral-code based room joining
* Real-time player synchronization
* Multiplayer gameplay synchronization
* Player disconnect/leave handling
* Automatic winner handling when a player leaves
* In-game chat

### 👥 Friends & Social System

* Send and receive friend requests
* Add friends
* Send game invitations
* Receive in-game invitation notifications
* Join games through invitations
* Friend-based multiplayer

### 👤 Player Profile

* Guest authentication
* Google authentication
* Player name customization
* Avatar selection
* Persistent player profile data

### 🔊 UI & Audio

* Complete game UI
* Main menu
* Lobby interface
* Profile interface
* Friend system interface
* Gameplay interface
* Settings
* Background music
* Gameplay sound effects
* Button and notification sounds
* Winner/loser feedback

---

## 🎮 Game Modes

| Mode                  | Description                                       |
| --------------------- | ------------------------------------------------- |
| **Play vs Play**      | Competitive multiplayer gameplay with 2–4 players |
| **Bot Mode**          | One real player competes against bot players      |
| **Play With Friends** | Create or join a lobby and play with friends      |

---

## 🃏 Game Rules

1. A standard **52-card deck** is distributed among the players.
2. Each player has their own card stack.
3. Players throw cards one by one into the center.
4. When the required matching card condition occurs, the corresponding player collects the center stack.
5. The game continues until the timer expires or a player obtains all cards.
6. When the timer expires, the player with the highest number of cards wins.
7. If a player obtains all cards before the timer expires, that player wins.
8. In multiplayer games, if a player leaves, the game handles the disconnection and determines the remaining game outcome according to the implemented rules.

---

## 🏗️ Architecture

The project follows a modular architecture separating gameplay, networking, authentication, Firebase services, social features, UI, and bot logic.

![MultiBlox Architecture](Architecture.png)

---

## 🔄 System Flow

![MultiBlox System Flow](System-Flow.png)

---

## 🛠️ Technologies Used

* **Unity**
* **C#**
* **Firebase Authentication**
* **Firebase Firestore**
* **Mirror Networking**
* **Telepathy Transport**
* **Photon Networking** where applicable
* **Git & GitHub**
* **TextMeshPro**
* **Unity UI System**

---

## 📂 Project Structure

```text
MultiBlox/
│
├── README.md
│
├── Architecture.png
├── System-Flow.png
│
├── Screenshots/
│   ├── MainMenu.png
│   ├── Login.png
│   ├── Profile.png
│   ├── PlayVsPlay.png
│   ├── BotMode.png
│   ├── Multiplayer.png
│   ├── Lobby.png
│   ├── Friends.png
│   ├── FriendRequest.png
│   ├── Invitation.png
│   ├── Chat.png
│   ├── Gameplay.png
│   └── WinnerScreen.png
│
├── Source-Code/
│   ├── Authentication/
│   ├── Gameplay/
│   ├── Bot/
│   ├── Networking/
│   ├── Firebase/
│   ├── Friends/
│   ├── Profile/
│   ├── Chat/
│   ├── UI/
│   └── Audio/
│
└── Build/
    └── MultiBlox.apk
```

---

## 📸 Screenshots

### Main Menu

![Main Menu](Screenshots/MainMenu.png)

### Gameplay

![Gameplay](Screenshots/Gameplay.png)

### Bot Mode

![Bot Mode](Screenshots/BotMode.png)

### Multiplayer

![Multiplayer](Screenshots/Multiplayer.png)

### Friends & Social System

![Friends](Screenshots/Friends.png)

> Additional gameplay and UI screenshots are available in the `Screenshots/` directory.

---

## 🔐 Authentication & Player Data

MultiBlox supports:

* Guest login
* Google authentication
* Persistent player profile
* Player name
* Avatar selection
* Friend data
* Friend requests
* Invitations
* Game-related user data

Firebase services are used where applicable for authentication and persistent cloud data.

---

## 🌐 Multiplayer Features

The multiplayer architecture handles:

```text
Player
   │
   ▼
Create / Join Lobby
   │
   ▼
Referral Code
   │
   ▼
Players Connected
   │
   ▼
Game Synchronization
   │
   ├── Player Actions
   ├── Card State
   ├── Turns
   ├── Game Events
   ├── Chat
   └── Player Disconnect
   │
   ▼
Winner Detection
```

---

## 🤖 Bot System

Bot gameplay uses dedicated bot controllers to simulate player actions.

The bot system integrates with the same core gameplay logic so that the game maintains consistent:

* Turn handling
* Card throwing
* Card collection
* Game state
* Timer handling
* Winner detection

---

## 📱 Build

A playable Android APK is provided in:

```text
Build/MultiBlox.apk
```

> For security and repository-size considerations, the complete Unity project and generated Unity folders are not included.

---

## 💻 Source Code

The source code is organized according to functionality:

| Directory         | Purpose                        |
| ----------------- | ------------------------------ |
| `Authentication/` | Login and authentication       |
| `Gameplay/`       | Core card-game mechanics       |
| `Bot/`            | Bot player logic               |
| `Networking/`     | Multiplayer networking         |
| `Firebase/`       | Firebase/Firestore integration |
| `Friends/`        | Friend and request systems     |
| `Profile/`        | Player profile and avatar      |
| `Chat/`           | In-game communication          |
| `UI/`             | User interface systems         |
| `Audio/`          | Audio and sound management     |

---

## 🎯 Technical Highlights

* Modular Unity/C# architecture
* Multiplayer state synchronization
* Bot player implementation
* Lobby and room management
* Firebase-backed player data
* Authentication integration
* Friend and invitation systems
* Real-time game interaction
* Player disconnect handling
* Event-driven UI interactions
* Persistent player profile data
* Scalable separation of gameplay and social systems

---

## 🧪 Testing

The project was tested across the major gameplay flows:

* Guest login
* Google login
* Profile customization
* 2-player games
* 3-player games
* 4-player games
* Bot matches
* Multiplayer lobby creation
* Lobby joining
* Friend requests
* Game invitations
* In-game chat
* Player leaving/disconnection
* Winner detection
* Timer-based game completion

---

## 📌 Project Status

**Project Type:** Unity Multiplayer Game
**Platform:** Android
**Engine:** Unity
**Language:** C#
**Status:** Functional prototype / portfolio project

---

## 👨‍💻 Developer

Developed as a complete Unity game project with a focus on:

* Game development
* Multiplayer networking
* Backend integration
* Firebase services
* UI/UX implementation
* Game-state management
* Bot development

---

## 📄 License

This project is provided for **portfolio and educational purposes**.

Please do not redistribute or commercially use the project's assets or code without permission.
