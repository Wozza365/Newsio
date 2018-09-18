import sqlite3
import time

connection = sqlite3.connect("server.db")
cursor = connection.cursor()


##CREATE EMPTY TABLE COMMANDS REMOVE ''' AT TOP AND BOTTOM
'''
##CREATE USERS
sql_command = """
CREATE TABLE user (
username VARCHAR(20) PRIMARY KEY,
password VARCHAR(20),
email VARCHAR(30));
"""

cursor.execute(sql_command)

##CREATE HIGHSCORES
sql_command = """
CREATE TABLE highscore (
username PRIMARY KEY,
highscore INTEGER,
FOREIGN KEY(username) REFERENCES user(username)
);
"""

cursor.execute(sql_command)

##CREATE GAMECOMBOS

sql_command = """
CREATE TABLE gamecombos (
date VARCHAR(8),
gamemode VARCHAR(20),
characterOne VARCHAR(20),
characterTwo VARCHAR(20),
location VARCHAR(20),
PRIMARY KEY (date, gamemode, characterOne, characterTwo, location)
);
"""
cursor.execute(sql_command)

connection.commit()
connection.close()

'''

#Add a new user to the database
def NewUser(username, password, email):
    try:
        connection = sqlite3.connect("server.db")
        cursor = connection.cursor()
        cursor.execute("INSERT INTO user VALUES (?, ?, ?)", (username, password, email))
        connection.commit()
        connection.close()
        return "VALID"
    except sqlite3.IntegrityError:
        return "INVAL"

#Login a user by checking if they exist
def Login(userInput, pwIn):
    connection = sqlite3.connect("server.db")
    cursor = connection.cursor()
    cursor.execute("SELECT * FROM user WHERE username=? AND password=?", (userInput, pwIn))
    idCorrect = cursor.fetchone()
    connection.commit()
    connection.close()
    
    if idCorrect:
        return "VALID"
    else:
        return "INVAL"

#Add a new highscore, add new or update if new is higher
def AddHighScore(user, score):
    connection = sqlite3.connect("server.db")
    cursor = connection.cursor()
    cursor.execute("SELECT * FROM highscore WHERE username=?", (user,))
    s = cursor.fetchall()
    
    if not s:
        cursor.execute("INSERT INTO highscore VALUES (?,?)", (user,score))
    
    elif int(score) > s[0][1]:
        cursor.execute("INSERT OR REPLACE INTO highscore VALUES (?,?)", (user,score))

    #cursor.execute("SELECT * FROM highscore WHERE username=?", (user,))
    #s = cursor.fetchall()
    connection.commit()
    connection.close()

#Add a new game combo to the database
def AddGameCombo(mode, char1, char2, location):
    currentDate = time.strftime("20" + "%y%m%d")
    connection = sqlite3.connect("server.db")
    cursor = connection.cursor()
    cursor.execute("INSERT OR REPLACE INTO gamecombos VALUES (?, ?, ?, ?, ?)", (currentDate, mode, char1, char2, location))
    connection.commit()
    connection.close()

#get the list of existing game combos
def GetGameCombos():
    connection = sqlite3.connect("server.db")
    cursor = connection.cursor()
    cursor.execute("SELECT * FROM gamecombos")
    result = cursor.fetchall()   
    connection.close()
    return result
