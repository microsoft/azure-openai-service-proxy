# @lru_cache_with_expiry(maxsize=64, ttl=600)
# async def expensive_async_function(arg1, arg2):
#     # Expensive computation here
#     await asyncio.sleep(1)
#     return arg1 + arg2

"""LRU Cache with Expiry"""

import functools
import time

import cachetools


class ExpiringLRUCache:
    """LRU Cache with Expiry"""

    def __init__(self, maxsize=128, ttl=600):
        self.cache = cachetools.LRUCache(maxsize)
        self.ttl = ttl
        self.timestamps = {}

    def get(self, key):
        """get value from cache"""
        if key in self.cache and time.time() - self.timestamps[key] < self.ttl:
            return self.cache[key]
        return None

    def set(self, key, value):
        """set key value pair to cache"""
        self.cache[key] = value
        self.timestamps[key] = time.time()


def lru_cache_with_expiry(maxsize=128, ttl=600):
    """LRU Cache with Expiry"""
    cache = ExpiringLRUCache(maxsize, ttl)

    def decorator(func):
        @functools.wraps(func)
        async def wrapper(*args, **kwargs):
            key = args + tuple(kwargs.items())
            result = cache.get(key)
            if result is None:
                result = await func(*args, **kwargs)
                cache.set(key, result)
            return result

        return wrapper

    return decorator
