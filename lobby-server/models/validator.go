package models

import validator "gopkg.in/go-playground/validator.v9"

type Validator struct {
	validate *validator.Validate
}

func NewValidator() *Validator {
	validate := validator.New()
	validate.RegisterCustomTypeFunc(validateMapString, MapString(""))
	return &Validator{
		validate: validate,
	}
}

func (v *Validator) Validate(i interface{}) error {
	return v.validate.Struct(i)
}
