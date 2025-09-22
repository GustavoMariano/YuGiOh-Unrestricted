# YuGiOh-Unrestricted

**YuGiOh-Unrestricted** is a personal learning project that simulates a multiplayer **Yu-Gi-Oh!** card game using modern .NET web technologies.  
The project is driven by nostalgia for the classic Yu-Gi-Oh! TCG and a desire to learn multiplayer game development.  
It provides an interactive platform for friendly duels and showcases real-time gameplay features built with cutting-edge web tech in a fun, nostalgic context.

---

## Features

- **Deck Builder** – Build and customize your own decks through a user-friendly web interface. Create the perfect deck from a vast card pool and save it for your next duel.  
- **Real-Time Duels** – Challenge other players in live duels with real-time gameplay. Game state is synchronized instantly between players using SignalR, making each duel feel responsive and authentic.  
- **Card Import via API** – Enjoy an up-to-date card database imported through an external API (e.g. YGOPRODeck).  
  All card data and images are loaded via external URLs at runtime – **no card images are stored in this repository**, ensuring the project stays lightweight and legally unencumbered.  
- **Online Lobby** – Meet and chat with fellow duelists in an online lobby. Set up matches, discuss strategies, or just hang out before you draw your next card in battle.  

---

## Technologies Used

- **Blazor Server** – Interactive front-end UI built with Blazor Server (ASP.NET Core), allowing rich dynamic web pages using C# and Razor.  
- **SignalR** – Real-time communication library for handling live duel interactions (sends game updates instantly to all players, enabling synchronized gameplay).  
- **Entity Framework Core (with SQLite)** – Handles server-side data management. Uses a lightweight SQLite database for storing persistent data like user info or deck lists.  
- **.NET Core (Latest)** – Leverages the power of modern .NET for a robust server and application structure, making it easy to extend and maintain.  

---

## Disclaimer

This is a **fan-made project** created for educational purposes and personal enjoyment.  
It is **not affiliated with or endorsed by Konami** or the official Yu-Gi-Oh! franchise in any way.  
All Yu-Gi-Oh! cards, and names are trademarks of their respective owners.  

---

## Author

Developed by **Gustavo Mariano**  
[GitHub](https://github.com/GustavoMariano) | [LinkedIn](https://www.linkedin.com/in/gustavo-mariano)

---

<div align="center">

*Trust in the Heart of the Cards.*  
*It’s time to duel!*  

</div>
