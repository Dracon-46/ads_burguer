let categoriaAtiva = '';  // Rastreamento da categoria ativa
let botaoAtivo = null;    // Rastreamento do botão ativo

function mostrarEsconder(id) {
    // Esconde todas as seções
    let sections = document.querySelectorAll('section');
    sections.forEach(function(section) {
        section.style.display = 'none';
    });

    // Mostra a seção desejada
    document.getElementById(id).style.display = 'block';

    // Remove o filtro ativo
    removerFiltro();
}

function trocarCor(button) {
    let icons = document.querySelectorAll('.selection i');
    let spans = document.querySelectorAll('.selection span');

    // Remove cor e classe de todos
    icons.forEach(icon => {
        icon.style.backgroundColor = '';
        icon.style.color = '';
        icon.classList.remove('active');
    });

    spans.forEach(span => {
        span.style.backgroundColor = '';
        span.style.color = '';
        span.classList.remove('active');
    });

    // Aplica cor apenas no botão clicado
    let icon = button.querySelector('i');
    let span = button.querySelector('span');
    if (icon) icon.classList.add('active');
    if (span) span.classList.add('active');

    // Remove o filtro ativo também
    removerFiltro();
}

function filtrarLanches(categoria, button) {
    let produtos = document.querySelectorAll('.produto');

    if (categoriaAtiva === categoria) {
        // Já está ativo, então remove
        produtos.forEach(produto => produto.style.display = 'inline-block');
        categoriaAtiva = '';
        if (botaoAtivo) botaoAtivo.classList.remove('active');
        botaoAtivo = null;
    } else {
        // Aplica novo filtro
        produtos.forEach(produto => {
            if (produto.classList.contains(categoria)) {
                produto.style.display = 'inline-block';
            } else {
                produto.style.display = 'none';
            }
        });
        if (botaoAtivo) botaoAtivo.classList.remove('active');
        button.classList.add('active');
        botaoAtivo = button;
        categoriaAtiva = categoria;
    }
}

function removerFiltro() {
    let produtos = document.querySelectorAll('.produto');
    produtos.forEach(produto => produto.style.display = 'inline-block');
    if (botaoAtivo) {
        botaoAtivo.classList.remove('active');
        botaoAtivo = null;
    }
    categoriaAtiva = '';
}
