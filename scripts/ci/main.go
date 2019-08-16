package main

import (
	"fmt"
	"net/http"

	"gopkg.in/go-playground/webhooks.v5/github"
)

func main() {
	hook, _ := github.New(github.Options.Secret("MyGitHubSuperSecretSecrect...?"))

	http.HandleFunc("/webhook", func(w http.ResponseWriter, r *http.Request) {
		payload, err := hook.Parse(r, github.ReleaseEvent, github.PullRequestEvent)
		if err != nil {
			if err == github.ErrEventNotFound {
				w.WriteHeader(400)
				return
			}
		}
		switch payload.(type) {

		case github.PushPayload:
			push := payload.(github.PushPayload)
			w.WriteHeader(400)
			// Do whatever you want from here...
			fmt.Printf("%+v", release)
		}
	})
	http.ListenAndServe(":3000", nil)
}
