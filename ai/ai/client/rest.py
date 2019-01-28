import requests

class RestClient:
    def __init__(self, token, server = 'http://minda.games:8080'):
        self.server = server
        self._session = requests.Session()
        self._session.headers.update({'Authorization': token})

    def get_rooms(self):
        r = self._session.get(self.server + '/rooms/')
        return r.json()

    def get_maps(self):
        r = self._session.get(self.server + '/maps/')
        return r.json()
