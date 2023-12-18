""" This module contains the RateLimit class which is used to rate limit the api calls """

# this class will be used to rate limit the api calls
# the class will use a dictionary to store the number of calls
# made by each user
# the key to the dictionary will be the users token
# the key properties include the number of calls made and the time of the last call
# the rate limit will be calcultated by the number of calls made in the last 10 seconds
# the user token properties will be deleted from the dictionary after 10 seconds of no calls

import time

CALL_RATE_PER_MINUTE = 120 * 12
CALL_RATE_PER_10_SECONDS = CALL_RATE_PER_MINUTE / 6


class RateLimit:
    """This class is used to rate limit the api calls"""

    def __init__(self):
        self.tokens = {}

    def is_call_rate_exceeded(self, token) -> bool:
        """This method is used to check if the rate limit has been exceeded"""
        # loop through the dictionary and delete any tokens older than 10 seconds

        current_time = time.time()

        for key in list(self.tokens.keys()):
            if current_time - self.tokens[key]["last_call"] > 10:
                del self.tokens[key]

        # if the token is already in the dictionary
        # increment the number of calls made by 1
        # else add the token to the dictionary and set the number of calls made to 1

        if token in self.tokens:
            self.tokens[token]["count"] += 1
        else:
            self.tokens[token] = {"count": 1, "last_call": time.time()}

        # if the number of calls made is greater than the rate limit
        # return true else return false

        if self.tokens[token]["count"] > CALL_RATE_PER_10_SECONDS:
            # set the last call time to now
            self.tokens[token]["last_call"] = current_time
            return True

        return False
