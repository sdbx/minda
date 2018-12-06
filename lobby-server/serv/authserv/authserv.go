package authserv

type AuthServ struct {
}

func Provide() *AuthServ {
	return &AuthServ{}
}
