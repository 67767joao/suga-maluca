# Cubo Automatico

Rotacao de cubo 3D na tela inteira, automatica a cada N minutos.

## Como obter o .exe (sem instalar nada)

1. Crie um repositorio publico no GitHub (ex.: cubo-automatico).
2. Faca upload de todos os arquivos deste pacote mantendo a estrutura de pastas.
3. Commit.
4. Aba Actions -> workflow "build" roda automaticamente (~2 min).
5. Clique no run verde -> baixe o artifact CuboAutomatico.
6. Extraia o zip -> CuboAutomatico.exe.
7. Duplo clique.

## Uso
- Intervalo (min): de quanto em quanto tempo dispara sozinho.
- Duracao (s): quanto tempo a rotacao leva.
- Iniciar / Pausar / Testar agora / Sair.

## Limitacoes reais
- Conteudo protegido por DRM aparece preto na captura (limitacao do Windows Graphics Capture).
- Multiplos monitores: animacao cobre apenas o monitor principal.
- A "rotacao" e uma transicao sobreposta com frame congelado — igual ao PowerPoint.