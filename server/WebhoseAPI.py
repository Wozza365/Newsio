#  extra layer tailored to this project to make interfacing  #
# with the webhose API easier and return results in a better #
#                format for the main program.                #

# REQUESTS REQUIRED:
# Update every 8 hours: 3 per day, 93 per month (assuming 0 downtime)
# ~10 requests per update
# 93 * 10 = 930
# max API calls per month = 1000

import webhose
import time
import random
import re

#declare globals
locations=[]
characters=[]
websites=[]
locationLastDates=[]
characterLastDates=[]
currentLocation=""
currentCharacterOne=""
currentCharacterTwo=""
currentGameMode=""
currentWebsite="dailymail.co.uk"
currentDate = "" 

#Current overrides text file
locations = ["London", "UK", "USA"]
gamemodes = ["Football", "Boxing", "X Factor"]
characters = ["Trump", "David Cameron", "The Queen"]
websites = ["dailymail.co.uk", "theguardian.com", "metro.co.uk", "huffingtonpost.co.uk"]

def InitWebhose():
    currentDate = time.strftime("20" + "%y-%m-%d")
    webhose.config(token="a2847cea-50a5-4443-97bc-6bce22f0cf7d")
    loc = open('locations.txt', 'U') #read locations from text file
    locations = loc.readlines()
    for index in range(len(locations)):
        locations[index] = locations[index][:-1]
    #print (locations)
    cha = open('characters.txt', 'U') #read characters from text file
    characters = cha.readlines()
    for index in range(len(characters)):
        characters[index] = characters[index][:-1]
    #print (characters)
    web = open('websites.txt', 'U')
    websites = web.readlines()
    for index in range(len(websites)):
        websites[index] = websites[index][:-1]
    #print (websites)
    
def SearchWebhose(term):
    return (webhose.search(term))

def GetCurrent(item): #find the latest item per category
    #create a query for webhose
    searchQuery = webhose.Query()
    searchQuery.site = currentWebsite
    searchQuery.language = "english"
    tempResults=[]
    dates=[]
    times=[]
    #search from locations
    if item.upper() == "LOCATION":
        for index in range(len(locations)):
            #add item to query and send query
            searchQuery.phrase = locations[index]
            searchQuery.title = locations[index]
            result = SearchWebhose(searchQuery)
            try:
                #get latest item (stored last in query result)
                print(locations[index] + (" " * (25-len(locations[index]))) + result.posts[len(result.posts) - 1].crawled)
                tempResults.append(result.posts[len(result.posts)-1].crawled)
            except IndexError: #index error occurs if 0 is returned, meaning no articles on topic in a long while
                pass
    #search from characters
    elif item.upper() == "CHARACTERS":
        for index in range(len(characters)):
            searchQuery.phrase = characters[index]
            searchQuery.title = characters[index]
            result = SearchWebhose(searchQuery)
            try:
                print(characters[index] + (" " * (25-len(characters[index]))) + result.posts[len(result.posts)-1].crawled)
                tempResults.append(result.posts[len(result.posts)-1].crawled)
            except IndexError:
                pass

    #search from gamemodes
    elif item.upper() == "GAMEMODE":
        for index in range(len(gamemodes)):
            searchQuery.phrase = gamemodes[index]
            searchQuery.title = gamemodes[index]
            result = SearchWebhose(searchQuery)
            try:
                print(gamemodes[index] + (" " * (25-len(gamemodes[index]))) + result.posts[len(result.posts)-1].crawled)
                tempResults.append(result.posts[index].crawled)
            except IndexError:
                pass

    #reformatting the time and dates from the results
    #into a friendlier format for the GetLatest function
    for index in range(len(tempResults)):
        tempResults[index] = tempResults[index].replace("-","")
        tempResults[index] = tempResults[index].replace(":","")
        tempResults[index] = tempResults[index][:-9]
        dates.append(tempResults[index].split('T')[0])
        times.append(tempResults[index].split('T')[1])

    latestIndex = GetLatest(dates,times) #find index of item with newest article

    #if none of the items have articles from today then randomize
    if (len(dates) == 0):
        if (item.upper() == "LOCATION"):     
            latestIndex = random.randint(0, len(locations) - 1)
        elif (item.upper() == "CHARACTERS"):
            latestIndex = random.randint(0, len(characters) - 1)
        elif (item.upper() == "GAMEMODE"):
            latestIndex = random.randint(0, len(gamemodes) - 1)
    return latestIndex

def GetLatest(dates, times):
    #assumes order is consistent with item order in list
    withCurrentDates=[]
    currDate = time.strftime("20" + "%y%m%d")
    
    #compare each of the dates with the current
    for index in range(len(dates)):
        if (dates[index] == currDate):
            withCurrentDates.append(index)

    #if only 1 article today then use it straight away
    if (len(withCurrentDates) == 1):
        return withCurrentDates[0]


    #if there is more than 1 today then calculate most recent by time.
    elif (len(withCurrentDates) > 1):
        bestTimeIndex=0
        highestTime=0
        for index in range(len(withCurrentDates)):
            if (int(times[withCurrentDates[index]]) > int(highestTime)):
                highestTime = times[withCurrentDates[index]]
                bestTimeIndex = index
        return bestTimeIndex

    #default to the first item if there is none dated today
    else:
        return 0
    
def InvokeWebhose(): #to be called every x hours to update the game
    #randomise site used for source
    ranSite = random.randint(0, len(websites) - 1)
    currentWebsite = websites[ranSite]

    #get the latest of each type and second character is offset of first
    currentLocation = locations[GetCurrent("LOCATION")]
    charInd = GetCurrent("CHARACTERS")
    currentCharacterOne = characters[charInd]
    try:
        currentCharacterTwo = characters[charInd+1]
    except IndexError:
        currentCharacterTwo = characters[charInd-1]
    currentGameMode = gamemodes[GetCurrent("GAMEMODE")]

    #create array of settings to return to main part of server
    settings = []
    settings.append(currentLocation)
    settings.append(currentCharacterOne)
    settings.append(currentCharacterTwo)
    settings.append(currentGameMode)
    settings.append(currentWebsite)
    return settings

def ReformatDate(date):
    date = date[:4] + '-' + date[4:]
    date = date[:7] + '-' + date[7:]
    return (date)
