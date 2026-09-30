-- Script de criação da tabela Produtos (SQLite)
CREATE TABLE IF NOT EXISTS Produtos (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome      TEXT    NOT NULL,
    Preco     REAL    NOT NULL CHECK (Preco >= 0),
    Estoque   INTEGER NOT NULL CHECK (Estoque >= 0),
    Categoria TEXT    NOT NULL
);
