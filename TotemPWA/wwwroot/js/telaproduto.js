// Funções existentes
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

// Nova função para mostrar a tela de personalização
function mostrarPersonalizacao(produto) {
    // Cria um modal de personalização dinâmico
    const modal = document.createElement('div');
    modal.className = 'personalizacao-modal';
    modal.innerHTML = `
        <div class="personalizacao-content">
            <div class="personalizacao-header">
                <h2>Personalizar ${produto.nome}</h2>
                <span class="close-personalizacao">&times;</span>
            </div>
            <div class="personalizacao-body">
                <div class="produto-info">
                    <img src="${produto.imagem}" alt="${produto.nome}">
                    <div>
                        <h3>${produto.nome}</h3>
                        <p class="preco-base">Preço base: R$ ${produto.preco.toFixed(2).replace('.', ',')}</p>
                    </div>
                </div>
                <div id="ingredientes-lista">
                    <!-- Ingredientes serão injetados aqui via JavaScript -->
                </div>
            </div>
            <div class="personalizacao-footer">
                <div class="resumo">
                    <p>Total: <span class="total-personalizado">R$ ${produto.preco.toFixed(2).replace('.', ',')}</span></p>
                </div>
                <div class="botoes">
                    <button class="cancelar-personalizacao">Cancelar</button>
                    <button class="confirmar-personalizacao">Adicionar ao Carrinho</button>
                </div>
            </div>
        </div>
    `;
    
    document.body.appendChild(modal);
    
    // Adiciona os ingredientes
    const ingredientesLista = modal.querySelector('#ingredientes-lista');
    ingredientesLista.innerHTML = `
        <div class="ingrediente" data-preco="3">
            <img src="/images/alface.png" alt="Alface">
            <div class="info">
                <p>Adicionar: Alface</p>
                <p>R$ 3,00</p>
            </div>
            <div class="controles">
                <button class="decrementar">-</button>
                <span class="quantidade">0</span>
                <button class="incrementar">+</button>
            </div>
        </div>
        <!-- Outros ingredientes aqui -->
    `;
    
    // Configura eventos
    modal.querySelector('.close-personalizacao').addEventListener('click', () => {
        document.body.removeChild(modal);
    });
    
    modal.querySelector('.cancelar-personalizacao').addEventListener('click', () => {
        document.body.removeChild(modal);
    });
    
    modal.querySelector('.confirmar-personalizacao').addEventListener('click', () => {
        // Lógica para adicionar ao carrinho
        adicionarAoCarrinho(produto);
        document.body.removeChild(modal);
    });
    
    // Adiciona overlay
    const overlay = document.createElement('div');
    overlay.className = 'modal-overlay';
    document.body.appendChild(overlay);
}

// Função para adicionar ao carrinho (simplificada)
function adicionarAoCarrinho(produto) {
    // Aqui você implementaria a lógica para adicionar ao carrinho
    console.log('Produto adicionado:', produto);
    // Atualiza o carrinho
    if (window.fastFoodCart) {
        window.fastFoodCart.addToCart(produto);
        window.fastFoodCart.updateCart();
    }
}

// No final do arquivo, modifique a inicialização para:
document.addEventListener('DOMContentLoaded', function() {
    document.querySelectorAll('.btn-produto').forEach(btn => {
        btn.addEventListener('click', function(e) {
            e.preventDefault();
            const produto = {
                nome: this.getAttribute('data-nome'),
                preco: parseFloat(this.getAttribute('data-preco')),
                imagem: this.getAttribute('data-imagem')
            };
            mostrarTelaPersonalizacao(produto);
        });
    });
});