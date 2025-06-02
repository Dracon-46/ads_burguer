// Variáveis de controle para filtros (Feature 2)
let categoriaAtiva = '';  // Rastreamento da categoria ativa
let botaoAtivo = null;    // Rastreamento do botão ativo

// Função para exibir ou esconder seções e remover filtros ativos
function mostrarEsconder(id) {
  // Esconde todas as seções
  let sections = document.querySelectorAll('section');
  sections.forEach(section => {
    section.style.display = 'none';
  });
  
  // Mostra a seção desejada
  document.getElementById(id).style.display = 'block';
  
  // Remove o filtro ativo, caso haja
  removerFiltro();
}

// Função para trocar a cor dos botões do menu
function trocarCor(button) {
  let icons = document.querySelectorAll('.selection i');
  let spans = document.querySelectorAll('.selection span');
  
  // Remove estilos de todos os botões
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
  
  // Aplica o estilo para o botão clicado
  let icon = button.querySelector('i');
  let span = button.querySelector('span');
  if (icon) icon.classList.add('active');
  if (span) span.classList.add('active');
  
  // Também remove qualquer filtro ativo
  removerFiltro();
}

// Função para filtrar os produtos por categoria
function filtrarLanches(categoria, button) {
  let produtos = document.querySelectorAll('.produto');
  
  if (categoriaAtiva === categoria) {
    // Se o filtro já está ativo, remove-o
    produtos.forEach(produto => produto.style.display = 'inline-block');
    categoriaAtiva = '';
    if (botaoAtivo) botaoAtivo.classList.remove('active');
    botaoAtivo = null;
  } else {
    // Aplica o novo filtro: exibe somente produtos que possuem a classe da categoria
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

// Função para remover qualquer filtro ativo
function removerFiltro() {
  let produtos = document.querySelectorAll('.produto');
  produtos.forEach(produto => produto.style.display = 'inline-block');
  if (botaoAtivo) {
    botaoAtivo.classList.remove('active');
    botaoAtivo = null;
  }
  categoriaAtiva = '';
}

// Função para exibir a tela de personalização do produto (Feature 1)
function mostrarPersonalizacao(produto) {
  // Cria o modal de personalização
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
  
  // Configura os eventos de fechamento do modal
  modal.querySelector('.close-personalizacao').addEventListener('click', () => {
    document.body.removeChild(modal);
    if (document.querySelector('.modal-overlay')) {
      document.body.removeChild(document.querySelector('.modal-overlay'));
    }
  });
  
  modal.querySelector('.cancelar-personalizacao').addEventListener('click', () => {
    document.body.removeChild(modal);
    if (document.querySelector('.modal-overlay')) {
      document.body.removeChild(document.querySelector('.modal-overlay'));
    }
  });
  
  modal.querySelector('.confirmar-personalizacao').addEventListener('click', () => {
    // Chama a função para adicionar o produto ao carrinho
    adicionarAoCarrinho(produto);
    document.body.removeChild(modal);
    if (document.querySelector('.modal-overlay')) {
      document.body.removeChild(document.querySelector('.modal-overlay'));
    }
  });
  
  // Cria e adiciona o overlay para o modal
  const overlay = document.createElement('div');
  overlay.className = 'modal-overlay';
  document.body.appendChild(overlay);
}

// Função para adicionar o produto ao carrinho (simplificada)
function adicionarAoCarrinho(produto) {
  console.log('Produto adicionado:', produto);
  // Aqui você implementaria a lógica real para adicionar o produto ao carrinho
  if (window.fastFoodCart) {
    window.fastFoodCart.addToCart(produto);
    window.fastFoodCart.updateCart();
  }
}

// Inicializa os eventos após o carregamento do DOM
document.addEventListener('DOMContentLoaded', function() {
  // Associa os eventos de clique dos botões de produtos para abrir a personalização
  document.querySelectorAll('.btn-produto').forEach(btn => {
    btn.addEventListener('click', function(e) {
      e.preventDefault();
      const produto = {
        nome: this.getAttribute('data-nome'),
        preco: parseFloat(this.getAttribute('data-preco')),
        imagem: this.getAttribute('data-imagem')
      };
      mostrarPersonalizacao(produto);
    });
  });
});
