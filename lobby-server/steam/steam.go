package steam

import (
	"encoding/json"
	"errors"
	"io/ioutil"
	"log"
	"net/http"
	"net/url"
	"strconv"
	"strings"
	"time"
)

const (
	steamURL       = "https://api.steampowered.com"
	iSteamMicroTxn = "ISteamMicroTxnSandbox"
)
const (
	methodGET  = "GET"
	methodPOST = "POST"
)

type Steam struct {
	client *http.Client
	appid  string
	key    string
}

func New(appid string, key string) *Steam {
	return &Steam{
		client: &http.Client{
			Timeout: 5 * time.Second,
		},
		key:   key,
		appid: appid,
	}
}

func (s *Steam) apiURL(routes ...string) string {
	return steamURL + "/" + strings.Join(routes, "/") + "/"
}

type errorResp struct {
	Response struct {
		Error *struct {
			ErrorCode int    `json:"errorcode"`
			ErrorDesc string `json:"errordesc"`
		} `json:"error"`
	} `json:"response"`
}

func (s *Steam) apiCall(method string, args map[string]string, routes ...string) ([]byte, error) {
	req, err := http.NewRequest(method, s.apiURL(routes...), nil)
	if err != nil {
		return nil, err
	}
	switch method {
	case methodGET:
		q := req.URL.Query()
		for key, value := range args {
			q.Set(key, value)
		}
		req.URL.RawQuery = q.Encode()
	case methodPOST:
		form := url.Values{}
		for key, value := range args {
			form.Set(key, value)
		}
		req, err = http.NewRequest(method, s.apiURL(routes...), strings.NewReader(form.Encode()))
		if err != nil {
			return nil, err
		}
		req.Header.Add("Content-Type", "application/x-www-form-urlencoded")
		req.Header.Add("Content-Length", strconv.Itoa(len(form.Encode())))
	}

	resp, err := s.client.Do(req)
	if err != nil {
		return nil, err
	}

	if resp.StatusCode != 200 && resp.StatusCode != 201 {
		msg, _ := ioutil.ReadAll(resp.Body)
		return nil, errors.New("steam error: http error " + resp.Status + string(msg))
	}

	buf, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		return nil, err
	}

	var resp2 errorResp
	err = json.Unmarshal(buf, &resp2)
	if err != nil {
		return nil, err
	}

	if resp2.Response.Error != nil {
		return nil, errors.New("steam error: " + resp2.Response.Error.ErrorDesc)
	}
	return buf, nil
}

type authenticateUserTicketResp struct {
	R struct {
		P struct {
			Result          string `json:"result"`
			SteamID         string `json:"steamid"`
			VacBanned       bool   `json:"vacbanned"`
			PublisherBanned bool   `json:"publisherbanned"`
		} `json:"params"`
	} `json:"response"`
}

func (s *Steam) AuthenticateUserTicket(ticket string) (string, error) {
	args := map[string]string{
		"appid":  s.appid,
		"key":    s.key,
		"ticket": ticket,
	}
	buf, err := s.apiCall(methodGET, args, "ISteamUserAuth", "AuthenticateUserTicket", "v1")
	if err != nil {
		return "", err
	}

	var resp authenticateUserTicketResp
	err = json.Unmarshal(buf, &resp)
	if err != nil {
		return "", err
	}
	if resp.R.P.Result != "OK" {
		return "", errors.New("steam error: result not OK")
	}

	return resp.R.P.SteamID, nil
}

type PlayerSummary struct {
	ID           string `json:"steamid"`
	Name         string `json:"personaname"`
	Avatar       string `json:"avatar"`
	AvatarMedium string `json:"avatarmedium"`
	AvatarFull   string `json:"avatarfull"`
}

type getPlayerSummaryResp struct {
	R struct {
		Players []PlayerSummary `json:"players`
	} `json:"response"`
}

func (s *Steam) GetPlayerSummary(id string) (PlayerSummary, error) {
	args := map[string]string{
		"appid":    s.appid,
		"key":      s.key,
		"steamids": id,
	}
	buf, err := s.apiCall(methodGET, args, "ISteamUser", "GetPlayerSummaries", "v2")
	if err != nil {
		return PlayerSummary{}, err
	}

	var resp getPlayerSummaryResp
	err = json.Unmarshal(buf, &resp)
	if err != nil {
		return PlayerSummary{}, err
	}
	if len(resp.R.Players) == 0 {
		return PlayerSummary{}, errors.New("steam error: no such user")
	}

	return resp.R.Players[0], nil
}

type Item struct {
	ID     int
	Name   string
	Qty    int
	Amount int
}

type initTxnResp struct {
	R struct {
		Result string `json:"result"`
	} `json:"response"`
}

func (s *Steam) InitTxn(orderid int, id string, items []Item) (int, error) {
	args := map[string]string{
		"appid":     s.appid,
		"key":       s.key,
		"orderid":   strconv.Itoa(orderid),
		"steamid":   id,
		"itemcount": strconv.Itoa(len(items)),
		"currency":  "USD",
		"language":  "en",
	}
	for i, item := range items {
		s := strconv.Itoa(i)
		args["itemid["+s+"]"] = strconv.Itoa(item.ID)
		args["qty["+s+"]"] = strconv.Itoa(item.Qty)
		args["amount["+s+"]"] = strconv.Itoa(item.Amount)
		args["description["+s+"]"] = item.Name
	}

	buf, err := s.apiCall(methodPOST, args, iSteamMicroTxn, "InitTxn", "v3")
	if err != nil {
		return 0, err
	}
	var resp initTxnResp
	log.Println(buf)
	err = json.Unmarshal(buf, &resp)
	if err != nil {
		return 0, err
	}
	if resp.R.Result != "OK" {
		return 0, errors.New("steam error: result not OK")
	}

	return 0, nil
}

type finTxnResp struct {
	R struct {
		Result string `json:"result"`
	} `json:"response"`
}

func (s *Steam) FinalizeTxn(orderid int) error {
	args := map[string]string{
		"appid":   s.appid,
		"key":     s.key,
		"orderid": strconv.Itoa(orderid),
	}

	buf, err := s.apiCall(methodPOST, args, iSteamMicroTxn, "FinalizeTxn", "v2")
	if err != nil {
		return err
	}

	var resp finTxnResp
	err = json.Unmarshal(buf, &resp)
	if err != nil {
		return err
	}
	if resp.R.Result != "OK" {
		return errors.New("steam error: result not OK")
	}
	return nil
}
