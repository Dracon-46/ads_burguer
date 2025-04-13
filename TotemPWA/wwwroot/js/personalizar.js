class PersonalizarPedido {
    constructor(produtoBase) {
        this.produtoBase = produtoBase;
        this.ingredientesAdicionais = [];
        this.total = produtoBase.preco;
        this.overlay = null;
        this.modal = null;
        this.lastFocusedElement = null;
        this.initModal();
    }

    initModal() {
        this.fecharOutrosModais();
        this.criarModal();
        this.setupIngredientes();
        this.setupEventListeners();
        this.adicionarOverlay();
        this.adicionarClasseBodyModalOpen();
        this.setupAriaLiveRegion();
    }

    criarModal() {
        // Remove modal existente se houver
        const existingModal = document.querySelector('.personalizacao-modal');
        if (existingModal) existingModal.remove();

        this.modal = document.createElement('div');
        this.modal.className = 'personalizacao-modal';
        this.modal.setAttribute('aria-modal', 'true');
        this.modal.setAttribute('role', 'dialog');
        this.modal.setAttribute('aria-labelledby', 'personalizacao-modal-title');
        
        this.modal.innerHTML = `
            <div class="personalizacao-content">
                <div class="personalizacao-header">
                    <h2 id="personalizacao-modal-title">Personalizar ${this.produtoBase.nome}</h2>
                    <button class="close-personalizacao" aria-label="Fechar personalização">&times;</button>
                </div>
                <div class="personalizacao-body">
                    <div class="produto-info">
                        <img src="${this.produtoBase.imagem}" alt="${this.produtoBase.nome}" loading="lazy">
                        <div>
                            <h3>${this.produtoBase.nome}</h3>
                            <p class="preco-base">Preço base: R$ ${this.formatarPreco(this.produtoBase.preco)}</p>
                        </div>
                    </div>
                    <div id="ingredientes-lista" tabindex="0"></div>
                </div>
                <div class="personalizacao-footer">
                    <div class="resumo">
                        <p>Total: <span class="total-personalizado" aria-live="polite">R$ ${this.formatarPreco(this.total)}</span></p>
                    </div>
                    <div class="botoes">
                        <button class="cancelar-personalizacao" aria-label="Cancelar personalização">Cancelar</button>
                        <button class="confirmar-personalizacao" aria-label="Confirmar pedido personalizado">Adicionar ao Carrinho</button>
                    </div>
                </div>
            </div>
        `;
        
        document.body.appendChild(this.modal);
        setTimeout(() => {
            this.modal.classList.add('active');
        }, 10);
    }

    setupAriaLiveRegion() {
        if (!document.getElementById('aria-live-personalizacao')) {
            const ariaLive = document.createElement('div');
            ariaLive.id = 'aria-live-personalizacao';
            ariaLive.setAttribute('aria-live', 'polite');
            ariaLive.setAttribute('aria-atomic', 'true');
            ariaLive.style.position = 'absolute';
            ariaLive.style.overflow = 'hidden';
            ariaLive.style.clip = 'rect(0 0 0 0)';
            ariaLive.style.height = '1px';
            ariaLive.style.width = '1px';
            ariaLive.style.margin = '-1px';
            ariaLive.style.padding = '0';
            ariaLive.style.border = '0';
            document.body.appendChild(ariaLive);
        }
    }

    formatarPreco(valor) {
        return valor.toFixed(2).replace('.', ',');
    }

    setupIngredientes() {
        const ingredientesLista = this.modal.querySelector('#ingredientes-lista');
        const ingredientesData = this.obterIngredientesDisponiveis();

        ingredientesLista.innerHTML = ingredientesData.map((ingrediente, index) => `
            <div class="ingrediente" data-nome="${ingrediente.nome}" data-preco="${ingrediente.preco}" style="--i: ${index}">
                <img src="${ingrediente.imagem}" alt="${ingrediente.nome}" loading="lazy">
                <div class="info">
                    <p>Adicionar: ${ingrediente.nome}</p>
                    <p>R$ ${this.formatarPreco(ingrediente.preco)}</p>
                </div>
                <div class="controles">
                    <button class="decrementar" aria-label="Remover ${ingrediente.nome}">
                        <svg aria-hidden="true" width="20" height="20" viewBox="0 0 24 24">
                            <path fill="currentColor" d="M19 12.998H5v-2h14z"/>
                        </svg>
                    </button>
                    <span class="quantidade" aria-live="polite">0</span>
                    <button class="incrementar" aria-label="Adicionar ${ingrediente.nome}">
                        <svg aria-hidden="true" width="20" height="20" viewBox="0 0 24 24">
                            <path fill="currentColor" d="M19 12.998h-6v6h-2v-6H5v-2h6v-6h2v6h6z"/>
                        </svg>
                    </button>
                </div>
            </div>
        `).join('');
    }

    obterIngredientesDisponiveis() {
        return [
            { nome: "Alface", preco: 3.00, imagem: "/images/alface.png" },
            { nome: "Tomate", preco: 3.00, imagem: "/images/tomate.png" },
            { nome: "Queijo", preco: 3.00, imagem: "/images/queijo.png" },
            { nome: "Bacon", preco: 5.00, imagem: "/images/bacon.png" },
            { nome: "Cebola", preco: 2.00, imagem: "/images/cebola.png" },
            { nome: "Molho Especial", preco: 4.00, imagem: "/images/molho.png" }
        ];
    }

    fecharOutrosModais() {
        if (window.fastFoodCart?.cartVisible) {
            window.fastFoodCart.toggleCart();
        }
        
        document.querySelectorAll('.modal-overlay, .personalizacao-modal').forEach(el => {
            if (el !== this.modal) el.remove();
        });
    }

    adicionarOverlay() {
        this.removeOverlay();
        
        this.overlay = document.createElement('div');
        this.overlay.className = 'modal-overlay';
        this.overlay.setAttribute('aria-hidden', 'true');
        this.overlay.addEventListener('click', () => this.fecharModal());
        document.body.appendChild(this.overlay);
        
        // Animação
        setTimeout(() => {
            this.overlay.classList.add('active');
        }, 10);
    }

    adicionarClasseBodyModalOpen() {
        document.body.classList.add('modal-open');
    }

    removerClasseBodyModalOpen() {
        document.body.classList.remove('modal-open');
    }

    fecharModal() {
        if (this.modal) {
            this.modal.classList.remove('active');
            document.removeEventListener('keydown', this.boundHandleKeyDown);
            
            setTimeout(() => {
                this.modal.remove();
                this.modal = null;
                this.removeOverlay();
                this.removerClasseBodyModalOpen();
                
                // Retorna o foco para o elemento que abriu o modal
                if (this.lastFocusedElement) {
                    this.lastFocusedElement.focus();
                }
            }, 300);
        }
    }

    removeOverlay() {
        if (this.overlay) {
            this.overlay.classList.remove('active');
            setTimeout(() => {
                if (this.overlay && document.body.contains(this.overlay)) {
                    document.body.removeChild(this.overlay);
                    this.overlay = null;
                }
            }, 300);
        }
    }

    setupEventListeners() {
        // Bind para poder remover o listener depois
        this.boundHandleKeyDown = this.handleKeyDown.bind(this);
        
        this.modal.querySelector('.close-personalizacao').addEventListener('click', () => this.fecharModal());
        this.modal.querySelector('.cancelar-personalizacao').addEventListener('click', () => this.fecharModal());
        this.modal.querySelector('.confirmar-personalizacao').addEventListener('click', () => this.confirmarPedido());

        document.addEventListener('keydown', this.boundHandleKeyDown);
        
        this.configurarControlesIngredientes();
        
        // Foco no primeiro elemento interativo
        setTimeout(() => {
            this.modal.querySelector('.close-personalizacao').focus();
        }, 50);
    }

    configurarControlesIngredientes() {
        this.modal.querySelectorAll('.ingrediente').forEach(ingrediente => {
            const decrementar = ingrediente.querySelector('.decrementar');
            const incrementar = ingrediente.querySelector('.incrementar');
            const quantidade = ingrediente.querySelector('.quantidade');
            const { nome, preco } = ingrediente.dataset;
            const precoNum = parseFloat(preco);

            decrementar.addEventListener('click', () => {
                this.ajustarQuantidade(nome, precoNum, parseInt(quantidade.textContent) - 1, quantidade);
            });
            
            incrementar.addEventListener('click', () => {
                this.ajustarQuantidade(nome, precoNum, parseInt(quantidade.textContent) + 1, quantidade);
            });
            
            // Suporte a teclado
            decrementar.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    this.ajustarQuantidade(nome, precoNum, parseInt(quantidade.textContent) - 1, quantidade);
                }
            });
            
            incrementar.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    this.ajustarQuantidade(nome, precoNum, parseInt(quantidade.textContent) + 1, quantidade);
                }
            });
        });
    }

    ajustarQuantidade(nome, preco, novaQuantidade, elementoQuantidade) {
        if (novaQuantidade > 10) {
            this.mostrarFeedback(`Você só pode adicionar até 10 unidades de ${nome}.`, 'error');
            return;
        }
    
        novaQuantidade = Math.max(0, novaQuantidade);
        elementoQuantidade.textContent = novaQuantidade;
        this.atualizarIngrediente(nome, preco, novaQuantidade);
        
        // Feedback para leitores de tela
        const ariaLive = document.getElementById('aria-live-personalizacao');
        if (ariaLive) {
            ariaLive.textContent = `${nome} quantidade alterada para ${novaQuantidade}`;
        }
    }

    handleKeyDown(e) {
        if (e.key === 'Escape') {
            this.fecharModal();
        }
        
        // Trapping focus dentro do modal
        if (e.key === 'Tab' && this.modal) {
            this.trapFocus(e);
        }
    }

    trapFocus(e) {
        const focusableElements = this.modal.querySelectorAll(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );
        const firstElement = focusableElements[0];
        const lastElement = focusableElements[focusableElements.length - 1];

        if (e.shiftKey && document.activeElement === firstElement) {
            lastElement.focus();
            e.preventDefault();
        } else if (!e.shiftKey && document.activeElement === lastElement) {
            firstElement.focus();
            e.preventDefault();
        }
    }

    atualizarIngrediente(nome, preco, quantidade) {
        const index = this.ingredientesAdicionais.findIndex(item => item.nome === nome);
        
        if (quantidade > 0) {
            if (index !== -1) {
                this.ingredientesAdicionais[index].quantidade = quantidade;
            } else {
                this.ingredientesAdicionais.push({ nome, preco, quantidade });
            }
        } else if (index !== -1) {
            this.ingredientesAdicionais.splice(index, 1);
        }
        
        this.calcularTotal();
    }

    calcularTotal() {
        const totalIngredientes = this.ingredientesAdicionais.reduce(
            (sum, item) => sum + (item.preco * item.quantidade), 
            0
        );
        this.total = this.produtoBase.preco + totalIngredientes;
        this.atualizarTotalNaTela();
    }

    atualizarTotalNaTela() {
        const elementoTotal = this.modal?.querySelector('.total-personalizado');
        if (elementoTotal) {
            elementoTotal.textContent = `R$ ${this.formatarPreco(this.total)}`;
        }
    }

// No método confirmarPedido da classe PersonalizarPedido
confirmarPedido() {
    const produtoFinal = this.criarProdutoFinal();
    
    if (window.fastFoodCart) {
        try {
            const confirmButton = this.modal.querySelector('.confirmar-personalizacao');
            const originalText = confirmButton.innerHTML;
            confirmButton.disabled = true;
            confirmButton.innerHTML = '<span class="spinner"></span> Adicionando...';
            
            // Garante que o produto tenha todas as propriedades necessárias
            const produtoParaCarrinho = {
                name: produtoFinal.name || this.produtoBase.nome,
                price: produtoFinal.price,
                image: produtoFinal.image || this.produtoBase.imagem,
                quantity: produtoFinal.quantity || 1,
                descricao: produtoFinal.descricao || this.produtoBase.nome
            };
            
            window.fastFoodCart.addToCart(produtoParaCarrinho);
            window.fastFoodCart.showAddedToCartFeedback(produtoParaCarrinho.name);
            this.fecharModal();
        } catch (error) {
            console.error('Erro ao adicionar ao carrinho:', error);
            this.mostrarFeedback('Erro ao adicionar ao carrinho', 'error');
        } finally {
            const confirmButton = this.modal.querySelector('.confirmar-personalizacao');
            if (confirmButton) {
                confirmButton.disabled = false;
                confirmButton.innerHTML = originalText;
            }
        }
    }
}

    criarProdutoFinal() {
        return {
            name: this.produtoBase.nome,
            price: this.total,
            image: this.produtoBase.imagem,
            quantity: 1,
            ingredientes: [...this.ingredientesAdicionais.filter(ing => ing.quantidade > 0)],
            descricao: this.gerarDescricaoPedido(),
            personalizado: this.ingredientesAdicionais.some(ing => ing.quantidade > 0)
        };
    }
    

    gerarDescricaoPedido() {
        let descricao = this.produtoBase.nome;
        const ingredientesAtivos = this.ingredientesAdicionais.filter(ing => ing.quantidade > 0);
        
        if (ingredientesAtivos.length > 0) {
            descricao += " com " + ingredientesAtivos
                .map(ing => `${ing.quantidade}x ${ing.nome}`)
                .join(', ');
        }
        
        return descricao;
    }

    mostrarFeedbackPedidoAdicionado(nomeProduto) {
        this.mostrarFeedback(`${nomeProduto} adicionado ao carrinho!`, 'success');
    }

    mostrarFeedback(mensagem, tipo = 'info') {
        // Remove feedback existente
        const existingFeedback = document.querySelector('.feedback-personalizacao');
        if (existingFeedback) existingFeedback.remove();

        const feedback = document.createElement('div');
        feedback.className = `feedback-personalizacao ${tipo}`;
        feedback.setAttribute('role', 'status');
        feedback.innerHTML = `
            <span class="feedback-icon">${tipo === 'success' ? '✓' : tipo === 'error' ? '⚠' : 'i'}</span>
            <span class="feedback-text">${mensagem}</span>
        `;
        document.body.appendChild(feedback);
        
        // Animação
        setTimeout(() => {
            feedback.classList.add('show');
            
            setTimeout(() => {
                feedback.classList.remove('show');
                setTimeout(() => feedback.remove(), 300);
            }, 3000);
        }, 10);
    }

    async mostrarDialogoConfirmacao(titulo, mensagem) {
        return new Promise((resolve) => {
            const dialog = document.createElement('div');
            dialog.className = 'dialogo-confirmacao';
            dialog.innerHTML = `
                <div class="dialogo-conteudo">
                    <h3>${titulo}</h3>
                    <p>${mensagem}</p>
                    <div class="dialogo-botoes">
                        <button class="dialogo-cancelar">Cancelar</button>
                        <button class="dialogo-confirmar">Confirmar</button>
                    </div>
                </div>
                <div class="dialogo-overlay"></div>
            `;
            
            document.body.appendChild(dialog);
            
            dialog.querySelector('.dialogo-cancelar').addEventListener('click', () => {
                dialog.remove();
                resolve(false);
            });
            
            dialog.querySelector('.dialogo-confirmar').addEventListener('click', () => {
                dialog.remove();
                resolve(true);
            });
            
            // Foco no primeiro botão
            setTimeout(() => {
                dialog.querySelector('.dialogo-cancelar').focus();
            }, 50);
        });
    }
}

// Função para ser chamada da tela de produtos
function mostrarPersonalizacao(produto) {
    // Salva o elemento com foco atual
    const lastFocusedElement = document.activeElement;
    
    const personalizacao = new PersonalizarPedido(produto);
    
    // Armazena o elemento com foco para restaurar depois
    personalizacao.lastFocusedElement = lastFocusedElement;
    
    // Adiciona estilos dinâmicos se não existirem
    if (!document.getElementById('personalizacao-styles')) {
        const styleElement = document.createElement('style');
        styleElement.id = 'personalizacao-styles';
        styleElement.textContent = `
            .personalizacao-modal {
                position: fixed;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
                width: 95%;
                max-width: 800px;
                max-height: 90vh;
                background: transparent;
                z-index: 1001;
                opacity: 0;
                transition: opacity 0.3s ease, transform 0.3s ease;
                will-change: transform, opacity;
            }
            
            .personalizacao-modal.active {
                opacity: 1;
            }
            
            .personalizacao-content {
                background: white;
                border-radius: 8px;
                box-shadow: 0 5px 20px rgba(0,0,0,0.2);
                overflow: hidden;
                transform: translateY(0);
                transition: transform 0.3s ease;
            }
            
            .ingrediente {
                animation: slideInUp 0.3s ease forwards;
                animation-delay: calc(var(--i) * 0.05s);
                opacity: 0;
            }
            
            @keyframes slideInUp {
                from {
                    transform: translateY(10px);
                    opacity: 0;
                }
                to {
                    transform: translateY(0);
                    opacity: 1;
                }
            }
            
            .feedback-personalizacao {
                position: fixed;
                bottom: 20px;
                left: 50%;
                transform: translateX(-50%);
                padding: 12px 24px;
                border-radius: 4px;
                box-shadow: 0 2px 10px rgba(0,0,0,0.2);
                display: flex;
                align-items: center;
                gap: 8px;
                opacity: 0;
                transition: opacity 0.3s ease;
                z-index: 1100;
            }
            
            .feedback-personalizacao.show {
                opacity: 1;
            }
            
            .feedback-personalizacao.success {
                background: #4CAF50;
                color: white;
            }
            
            .feedback-personalizacao.error {
                background: #F44336;
                color: white;
            }
            
            .feedback-personalizacao.info {
                background: #2196F3;
                color: white;
            }
            
            .dialogo-confirmacao {
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                display: flex;
                align-items: center;
                justify-content: center;
                z-index: 1200;
            }
            
            .dialogo-overlay {
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(0,0,0,0.5);
                z-index: -1;
            }
            
            .dialogo-conteudo {
                background: white;
                border-radius: 8px;
                padding: 20px;
                width: 90%;
                max-width: 400px;
                box-shadow: 0 5px 20px rgba(0,0,0,0.2);
                z-index: 1;
            }
            
            .dialogo-botoes {
                display: flex;
                justify-content: flex-end;
                gap: 10px;
                margin-top: 20px;
            }
            
            .spinner {
                display: inline-block;
                width: 16px;
                height: 16px;
                border: 2px solid rgba(255,255,255,0.3);
                border-radius: 50%;
                border-top-color: white;
                animation: spin 1s ease-in-out infinite;
            }
            
            @keyframes spin {
                to { transform: rotate(360deg); }
            }
        `;
        document.head.appendChild(styleElement);
    }
}