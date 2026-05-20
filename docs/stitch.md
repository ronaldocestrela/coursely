# Design com Google Stitch (MCP)

Este projeto usa **[Google Stitch](https://stitch.withgoogle.com/projects/13905040481242586827)** como fonte de **layout, fluxos de UI e exploração de telas**, integrada ao fluxo de trabalho via **MCP Stitch** no Cursor.

## Projeto Stitch de referência

- URL: https://stitch.withgoogle.com/projects/13905040481242586827

Mantenha esse projeto atualizado quando houver decisões visuais ou de navegação relevantes para o produto.

## Como usar no desenvolvimento

1. **Consultar o Stitch** antes de implementar telas novas ou grandes mudanças de UX no [`frontend/`](../frontend/) (React, Tailwind v4, shadcn/ui).
2. **Traduzir para código** usando componentes existentes (`frontend/src/components/ui`, layouts em `frontend/src/layouts`) e tokens/tema já configurados pelo shadcn; evitar copiar valores soltos quando já existir equivalente no tema.
3. **Registrar divergências**: quando o código precisar diferir do Stitch (performance, acessibilidade, limite da biblioteca), documente brevemente no PR ou na doc da feature.

## Documentação relacionada

- Regras gerais do produto e stack: [`agents.md`](../agents.md)
- Sequência de entregas: [`roadmap.md`](../roadmap.md)
