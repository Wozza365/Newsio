#main portion of the server

import socket
import threading
import time
from apscheduler.scheduler import Scheduler
import urllib.request
import WebhoseAPI
import SQLInterface

#start scheduler
sched = Scheduler()
sched.start()

#global variables
tempSettings=[]
currentSettings=[]
currentGameMode=""
currentLocation=""
currentCharacterOne=""
currentCharacterTwo=""
currentDate=""
currentTime=""

WebhoseAPI.InitWebhose()

#receive function
def Receive(connection):
    try:
        r = connection.recv(1024).decode()
        return r
    except ConnectionResetError:
        return ConnectionResetError

#send function
def Transmit(connection, data):
    connection.send(data.encode())

#used continuously to listen for new connections logging in or registering
def NewConnections():
    #set the server to always listen to new users
    while True:
        #initial setup and communication attempts
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM, 0)
        host = "127.0.0.1"
        s.bind((host, 4000))
        s.listen(5)
        c, addr = s.accept()
        Transmit(c, "HELLO")

        #continue login attempts until successful
        loggedIn = False
        while loggedIn == False:     
            data = Receive(c)
            #attempt to login the user by querying the database
            if (data[:5] == "LOGIN"):
                username = data[6:]
                Transmit(c, "VALID")
                data = Receive(c)
                password = data[5:]
                result = SQLInterface.Login(username, password)
                Transmit(c, result)
                if result == "VALID":
                    loggedIn = True

            #register the user to the database
            elif (data[:5] == "REGIS"):
                username = data[6:]
                Transmit(c, "VALID")
                data = Receive(c)
                password = data[5:]
                Transmit(c, "VALID")
                data = Receive(c)
                email = data[6:]
                result = SQLInterface.NewUser(username, password, email)
                Transmit(c, result)
                if result == "VALID":
                    loggedIn = True

        #pass of the client to its own thread
        t = threading.Thread(target=ClientCommunication, args=(c,username))
        t.start()

#used to keep in contact with clients after they're logged in
def ClientCommunication(c,username):
    #loop to allow game to be replayed
    isDisconnected = False   
    while isDisconnected == False:
        try:
            data = Receive(c)

            #send the list of previously created gamemodes from db
            if (data[:6] == "CUSTOM"):
                playersSPCustom.append(c)
                listedGames = SQLInterface.GetGameCombos()
                Transmit(c, "LEN:"+str(len(listedGames)))
                Receive(c)
                #print (listedGames)
                for i in listedGames:
                    item = ""
                    for j in i:
                        item = item + j+ ","
                    Transmit(c, item)
                    Receive(c)

            #send the current settings
            elif (data[:6] == "CURREN"):
                playersSP.append(c)
                item = currentDate + "," + currentGameMode + "," + currentCharacterOne + ","+ currentCharacterTwo + "," + currentLocation + ","
                Transmit(c, item)

            elif (data[:6] == "HIGHSC"):
                SQLInterface.AddHighScore(username, data.split(":")[1])
                Transmit(c, "VALID")
            
        except: #usually a disconnect error so break the loop
            isDisconnected = True

#Update game settings from webhose
def UpdateGame():
    tempSettings.append(WebhoseAPI.InvokeWebhose())
    #print (currentSettings)
    currentDate = time.strftime("20" + "%y-%m-%d")
    currentTime = time.strftime("%H:%M:%S")
    print (currentDate + " - " + currentTime + " Server updated.")

playersSP=[]
playersMP=[]
playersSPCustom=[]

#job = sched.add_interval_job(UpdateGame, hours=4)#uncomment to allow server to update regularly
#startup webhose and update from it
WebhoseAPI.InitWebhose()
UpdateGame()

#print settings after updating
print ("Location: " + tempSettings[0][0])
print ("Player One: " + tempSettings[0][1])
print ("Player Two: " + tempSettings[0][2])
print ("Game mode: " + tempSettings[0][3])
print ("Website used: " + tempSettings[0][4])
currentSettings = tempSettings[0]
currentLocation = tempSettings[0][0]
currentCharacterOne = tempSettings[0][1]
currentCharacterTwo = tempSettings[0][2]
currentGameMode = tempSettings[0][3]
currentDate = time.strftime("%d%m%y")

#start listening for connections on a new thread
connections = threading.Thread(target = NewConnections, args = ())
connections.start()
