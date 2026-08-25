# Sem refresh token no MVP

Autenticação usa apenas um access token JWT — sem refresh token — no MVP. Um refresh token
com revogação/rotation exigiria infraestrutura extra (armazenamento server-side, rotation)
sem valor claro no estágio de validação com poucos early adopters. Optamos por um JWT de
vida mais longa (ex.: 7 dias) para evitar relogins frequentes. Ao crescer a base de
usuários, revisitar com refresh token + revogação, para reduzir a janela de exposição de um
token vazado.
