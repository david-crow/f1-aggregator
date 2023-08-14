# Formula 1 Aggregator

## Overview

This repo contains my courselong project for Microsoft's Software and Systems Academy. It serves to reinforce my C# skills while combining my love for Formula 1. In its current state, this console app scrapes [F1Calendar.com](https://f1calendar.com) and [Formula1.com](https://www.formula1.com/en/results.html) to present the race calendar, race results, and team/constructor standings in a simplified format.

In the future, I'll update this codebase to use:
1. .NET MAUI, which enables a graphical user interface, and
2. Microsoft SQL Server, which allows me to store data in a relational database.

## Implementation Details

At startup, the app greets the user and reports their currently recorded time. The user is presented with a menu, from which they can choose to view the schedule for the next race weekend or for the remaining season as a whole, recent results, or the current driver or constructor standings. Additionally, the user can choose to clear the output window or quit the program.

Each of the menu options do as expected, but they do so by scraping the relevant data from the websites mentioned above. The raw HTML is parsed into JSON objects which are then saved to files for later retrieval. Data are output in dynamically sized tables, ensuring clean information presentation for the user.