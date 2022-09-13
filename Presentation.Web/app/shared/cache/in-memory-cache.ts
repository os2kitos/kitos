module Kitos.Shared.Caching {

    interface ICacheEntry {
        expiresAt: Date,
        entry: any
    }

    export type InMemoryCache = Record<string, ICacheEntry>;

    const sharedCache: InMemoryCache = {}


    export interface IInMemoryCacheService {
        getEntry<T>(key: string): T | null
        setEntry<T>(key: string, entry: T, expireAt: Date): void
        deleteEntry(key: string): void
        clear(): void
    }

    class InMemoryCacheService implements IInMemoryCacheService {
        static $inject = ["sharedInMemoryCache"];
        constructor(private readonly sharedInMemoryCache: InMemoryCache) { }

        getEntry<T>(key: string): T {
            if (!!key) {
                const value = this.sharedInMemoryCache[key];
                if (value != undefined) {
                    const expired = Date.now() > value.expiresAt.getTime();
                    if (expired) {
                        //Kill expired entry
                        this.deleteEntry(key);
                    } else {

                        // Return persisted value
                        return <T>value.entry
                    }
                }
            }
            return null;
        }
        setEntry<T>(key: string, entry: T, expireAt: Date) {
            if (!key) {
                throw "Key must be defined";
            } else if (!expireAt) {
                throw "expireAt must be defined";
            }
            this.sharedInMemoryCache[key] = {
                entry: entry,
                expiresAt: expireAt
            };
        }
        deleteEntry(key: string) {
            delete this.sharedInMemoryCache[key];
        }

        clear() {
            //Delete all cached state
            for (const cacheKey in this.sharedInMemoryCache) {
                if (Object.prototype.hasOwnProperty.call(this.sharedInMemoryCache, cacheKey)) {
                    this.deleteEntry(cacheKey);
                }
            }
        }
    }

    app.constant("sharedInMemoryCache", sharedCache);
    app.service("inMemoryCacheService", InMemoryCacheService);

}