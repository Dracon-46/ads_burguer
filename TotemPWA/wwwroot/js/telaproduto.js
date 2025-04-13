function mostrarEsconder(id) {
    let sections = document.querySelectorAll('section');
    for (let i = 0; i < sections.length; i++) {
        sections[i].style.display = 'none';
    }
    document.getElementById(id).style.display = 'block';
}

function trocarCor(button) {
    
        let icons = document.querySelectorAll('.selection i');
        let spans = document.querySelectorAll('.selection span');
        icons.forEach(function(icon) {
            icon.style.backgroundColor = '';
            icon.style.color = '';
            icon.classList.remove('active');
        });
        
        spans.forEach(function(span) {
            span.style.backgroundColor = '';
            span.style.color = '';
            span.classList.remove('active');
           
        });

        let icon = button.querySelector('i');
        let span = button.querySelector('span');
        span.classList.add('active');
        icon.classList.add('active');
    

}
function trocaCorFiltro(button) {
    // Remove a classe 'active' de todos os botões
    let buttons = document.querySelectorAll('.filtro button');
    buttons.forEach(function(button) {
        button.classList.remove('active');
    });

    // Adiciona a classe 'active' no botão clicado
    button.classList.add('active');
}

function filtrarLanches(categoria) {
    // Seleciona todos os lanches
    let produtos = document.querySelectorAll('.produto');
    
    // Exibe ou oculta os lanches com base na categoria
    produtos.forEach(function(produto) {
        // Verifica se o produto tem a classe correspondente à categoria
        if (produto.classList.contains(categoria)) {
            produto.style.display = 'block'; // Exibe o produto
        } else {
            produto.style.display = 'none'; // Oculta o produto
        }
    });
}
