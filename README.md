# ThrowBall ☄️

## Intro ℹ️

ThrowBall is intended to be a common network and class library to be shared among all other microservices in the project. 
The reason behind that is to preserve consistency in the class definition of json model and encoded packets which can be seen under *Models* directory.

THe development is done purely in C# without any extra dependencies.

## Current State
The library already has a basic functionality of handling multiple clients, error handling and manual disconnection.
The demo can be found in folder. Although the proper load has not yet been done, manually launching 30 clients seems to work fine.

## Contribution 🧟‍♂️
Make sure a review changes and do not introduce backward incompatible changes which could affect the network modules of others microservices.

## License 📑
The code is licensed under the GNU General Public License v2.0