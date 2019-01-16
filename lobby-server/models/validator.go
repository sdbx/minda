package models

import validator "gopkg.in/go-playground/validator.v9"

var Validate *validator.Validate

func init() {
	Validate = validator.New()
	Validate.RegisterCustomTypeFunc(validateMapString, MapString(""))
}
