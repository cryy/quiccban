import * as signalR from '@aspnet/signalr';
import { EventEmitter } from 'events';
import { ICase } from '../entities/guild/ICase';
import { ISelfUser } from '../entities/user/ISelfUser';
import * as shortid from 'shortid';


interface EListener {
    name: string;
    method: any;
}

export class APIClient {


    private _isReady: boolean;
    private _websocket: signalR.HubConnection;
    private _eventEmitter: EventEmitter;
    private _caseCache: ICase[];
    private _didFetchRecentCases: boolean;


    private _eventTracker: { [id: string] : EListener };
    

    constructor() {
        this._eventEmitter = new EventEmitter();
        this._isReady = false;
        this._websocket = new signalR.HubConnectionBuilder()
            .withUrl("/api/ws")
            .build();
        this._caseCache = [];
        this._didFetchRecentCases = false;

        this._eventTracker = {};
        
        this._websocket.on("NEW_CASE", d => {
            var data = d as ICase;

            this._caseCache = [data, ...this._caseCache];

            this._eventEmitter.emit('NEW_CASE', data);
        });

        this._websocket.start();
        alert("ctr");
    }

    get wsConnectionState() {
        return this._websocket.state;
    }

    get isReady() {
        return this._isReady;
    }

    get caseCache() {
        return this._caseCache;
    }


    public removeListener(id: string) {

        let instance = this._eventTracker[id];

        this._eventEmitter.removeListener(instance.name, instance.method);

        delete this._eventTracker[id];
    }


    async getSelfInfo() {

        let result = await fetch("/api/context/user", { method: "GET" });

        if (result.status !== 200 || result.redirected)
            throw new Error("API: Couldn't get information about self.");

        return result.json() as Promise<ISelfUser>;

    }

    async getRecentCases() {


        if (!this._didFetchRecentCases) {

            let result = await fetch("/api/cases/recent", { method: "GET" });

            if (result.status !== 200 || result.redirected)
                return Promise.reject("API: Couldn't get recent cases.");

            let cases = await result.json() as ICase[];

            cases.forEach(c => {
                if (this._caseCache.findIndex(x => x.id === c.id && x.guildId === c.guildId) === -1)
                    this._caseCache = [c, ...this._caseCache];
            });

            this._didFetchRecentCases = true;

            return cases;

        }

        return this._caseCache.sort((a, b) => a.unixTimestamp.localeCompare(b.unixTimestamp)).reverse().slice(0, 10);

    }






    public onNewCase(method: (arg: ICase) => void): string {
        let id = shortid.generate();
        this._eventTracker[id] = { name: 'NEW_CASE', method: method };
        this._eventEmitter.on('NEW_CASE', method);
        return id;
    }


    public onClose(method: (error?: Error | undefined) => void): string {
        let id = shortid.generate();
        this._eventTracker[id] = { name: 'WSCLOSE', method: method };
        this._websocket.onclose(method);
        return id;
    }













}
