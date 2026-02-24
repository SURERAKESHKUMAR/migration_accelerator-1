provider "aws" {
  region = "us-east-1"
}

variable "environment" {}
variable "secret_name" {}
variable "secrets" {
  type = map(string)
}

resource "aws_secretsmanager_secret" "this" {
  name = var.secret_name
}

resource "aws_secretsmanager_secret_version" "this" {
  secret_id     = aws_secretsmanager_secret.this.id
  secret_string = jsonencode(var.secrets)
}
