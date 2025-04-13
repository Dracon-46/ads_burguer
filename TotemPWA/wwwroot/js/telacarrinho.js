class FastFoodCart {
    constructor() {
        this.cart = [];
        this.cartVisible = false;
        this.overlay = null;
        this.lastFocusedElement = null;
        this.initCart();
    }

    initCart() {
        this.createCartModal();
        this.setupProductClickHandlers();
        this.setupCartButton();
        this.updateCart();
        this.setupKeydownListener();
    }

    createCartModal() {
        // Remove modal existente se houver
        const existingModal = document.getElementById('cart-modal');
        if (existingModal) existingModal.remove();

        this.cartModal = document.createElement('div');
        this.cartModal.id = 'cart-modal';
        this.cartModal.className = 'cart-modal';
        this.cartModal.setAttribute('aria-modal', 'true');
        this.cartModal.setAttribute('role', 'dialog');
        this.cartModal.setAttribute('aria-labelledby', 'cart-modal-title');
        this.cartModal.style.display = 'none';
        
        this.cartModal.innerHTML = `
            <div class="cart-content">
                <div class="cart-header">
                    <h2 id="cart-modal-title">Seu Carrinho</h2>
                    <button class="close-cart" aria-label="Fechar carrinho">&times;</button>
                </div>
                <div class="cart-items" tabindex="0"></div>
                <div class="cart-summary">
                    <p>Total de itens: <span id="modal-total-items" aria-live="polite">0</span></p>
                    <p>Total do pedido: <span id="modal-total-price" aria-live="polite">R$ 0,00</span></p>
                </div>
                <div class="cart-actions">
                    <button class="cancel-button" aria-label="Fechar carrinho">Fechar</button>
                    <button class="confirm-button" aria-label="Confirmar pedido">Confirmar Pedido</button>
                </div>
            </div>
        `;
        
        document.body.appendChild(this.cartModal);
        
        // Event listeners
        this.cartModal.querySelector('.close-cart').addEventListener('click', () => this.toggleCart());
        this.cartModal.querySelector('.cancel-button').addEventListener('click', () => this.toggleCart());
        this.cartModal.querySelector('.confirm-button').addEventListener('click', () => this.confirmOrder());
    }

    setupCartButton() {
        const cartButton = document.querySelector('.btn-car');
        const viewCartText = document.querySelector('.view');
        
        const handleCartClick = (e) => {
            if (e) e.preventDefault();
            this.toggleCart();
        };

        if (cartButton) {
            cartButton.href = 'javascript:void(0)';
            cartButton.addEventListener('click', handleCartClick);
            cartButton.setAttribute('aria-haspopup', 'dialog');
            cartButton.setAttribute('aria-expanded', 'false');
        }
        
        if (viewCartText) {
            viewCartText.style.cursor = 'pointer';
            viewCartText.addEventListener('click', handleCartClick);
            viewCartText.setAttribute('role', 'button');
            viewCartText.setAttribute('tabindex', '0');
            viewCartText.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    handleCartClick(e);
                }
            });
        }
    }

    setupProductClickHandlers() {
        document.querySelectorAll('.produto button').forEach(button => {
            button.addEventListener('click', (e) => {
                e.preventDefault();
                const product = this.getProductData(e.target.closest('.produto'));
                this.addToCart(product);
                this.showAddedToCartFeedback(product.name);
                
                // Atualiza ARIA live region para leitores de tela
                const ariaLive = document.getElementById('cart-aria-live') || 
                    this.createAriaLiveRegion();
                ariaLive.textContent = `${product.name} adicionado ao carrinho`;
                setTimeout(() => ariaLive.textContent = '', 2000);
            });
        });
    }

    createAriaLiveRegion() {
        const ariaLive = document.createElement('div');
        ariaLive.id = 'cart-aria-live';
        ariaLive.setAttribute('aria-live', 'polite');
        ariaLive.style.position = 'absolute';
        ariaLive.style.overflow = 'hidden';
        ariaLive.style.clip = 'rect(0 0 0 0)';
        ariaLive.style.height = '1px';
        ariaLive.style.width = '1px';
        ariaLive.style.margin = '-1px';
        ariaLive.style.padding = '0';
        ariaLive.style.border = '0';
        document.body.appendChild(ariaLive);
        return ariaLive;
    }

    setupKeydownListener() {
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.cartVisible) {
                this.toggleCart();
            }
            
            // Trapping focus dentro do modal quando aberto
            if (e.key === 'Tab' && this.cartVisible) {
                this.trapFocus(e);
            }
        });
    }

    trapFocus(e) {
        const focusableElements = this.cartModal.querySelectorAll(
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

    toggleCart() {
        this.cartVisible = !this.cartVisible;
        
        if (this.cartVisible) {
            this.lastFocusedElement = document.activeElement;
            this.cartModal.style.display = 'block';
            this.addOverlay();
            document.body.classList.add('modal-open');
            
            // Força o recálculo do layout para animação
            void this.cartModal.offsetWidth;
            this.cartModal.classList.add('active');
            
            // Atualiza ARIA attributes
            document.querySelector('.btn-car')?.setAttribute('aria-expanded', 'true');
            
            // Foco no primeiro elemento interativo
            setTimeout(() => {
                this.cartModal.querySelector('.close-cart').focus();
            }, 50);
        } else {
            this.cartModal.classList.remove('active');
            document.body.classList.remove('modal-open');
            
            // Atualiza ARIA attributes
            document.querySelector('.btn-car')?.setAttribute('aria-expanded', 'false');
            
            setTimeout(() => {
                this.cartModal.style.display = 'none';
                this.removeOverlay();
                
                // Retorna o foco para o elemento que abriu o modal
                if (this.lastFocusedElement) {
                    this.lastFocusedElement.focus();
                }
            }, 300);
        }
    }

    addOverlay() {
        this.removeOverlay();
        
        this.overlay = document.createElement('div');
        this.overlay.className = 'modal-overlay active';
        this.overlay.addEventListener('click', () => this.toggleCart());
        this.overlay.setAttribute('aria-hidden', 'true');
        document.body.appendChild(this.overlay);
    }

    removeOverlay() {
        const overlay = this.overlay || document.querySelector('.modal-overlay');
        if (overlay) {
            overlay.classList.remove('active');
            setTimeout(() => {
                if (overlay && document.body.contains(overlay)) {
                    document.body.removeChild(overlay);
                    this.overlay = null;
                }
            }, 300);
        }
    }

    addToCart(product) {
        // Remove propriedades desnecessárias para comparação
        const simpleProduct = {
            name: product.name,
            price: product.price,
            image: product.image
        };
        
        const existingItem = this.cart.find(item => 
            item.name === simpleProduct.name &&
            item.price === simpleProduct.price &&
            item.image === simpleProduct.image
        );
        
        if (existingItem) {
            existingItem.quantity++;
        } else {
            this.cart.push({
                ...product,
                quantity: product.quantity || 1
            });
        }
        
        this.updateCart();
    }

    removeFromCart(productName, removeCompletely = false) {
        const index = this.cart.findIndex(item => item.name === productName);
        if (index !== -1) {
            if (removeCompletely || this.cart[index].quantity === 1) {
                this.cart.splice(index, 1);
            } else {
                this.cart[index].quantity--;
            }
        }
        this.updateCart();
    }

    updateCart() {
        const itemsContainer = this.cartModal.querySelector('.cart-items');
        itemsContainer.innerHTML = '';
        
        if (this.cart.length === 0) {
            itemsContainer.innerHTML = `
                <div class="empty-cart">
                    <svg aria-hidden="true" width="48" height="48" viewBox="0 0 24 24">
                        <path fill="currentColor" d="M7 22q-.825 0-1.412-.587Q5 20.825 5 20q0-.825.588-1.413Q6.175 18 7 18t1.412.587Q9 19.175 9 20q0 .825-.588 1.413Q7.825 22 7 22Zm10 0q-.825 0-1.412-.587Q15 20.825 15 20q0-.825.588-1.413Q16.175 18 17 18t1.413.587Q19 19.175 19 20q0 .825-.587 1.413Q17.825 22 17 22ZM7 17q-1.125 0-1.7-.988-.575-.987-.05-1.962L6.6 11.6L3 4H2q-.425 0-.712-.288Q1 3.425 1 3t.288-.713Q1.575 2 2 2h1.625q.275 0 .525.15t.375.425L5.2 4h14.75q.675 0 .925.5t-.025 1.05l-3.55 6.4q-.275.5-.737.775Q16 13 15.45 13H8.1L7 15h11q.425 0 .713.287q.287.288.287.713t-.287.712Q18.425 17 18 17Z"/>
                    </svg>
                    <p>Seu carrinho está vazio</p>
                </div>
            `;
        } else {
            this.cart.forEach((item, index) => {
                const itemElement = document.createElement('div');
                itemElement.className = 'cart-item';
                itemElement.setAttribute('data-product-id', item.name.replace(/\s+/g, '-').toLowerCase());
                itemElement.innerHTML = `
                    <div class="item-info">
                        <img src="${item.image}" alt="${item.name}" loading="lazy">
                        <div>
                            <h4>${item.name}</h4>
                            <p>R$ ${this.formatPrice(item.price)}</p>
                        </div>
                    </div>
                    <div class="item-quantity">
                        <button class="quantity-btn decrease" aria-label="Reduzir quantidade de ${item.name}">-</button>
                        <span class="item-quantity-value" aria-live="polite">${item.quantity}</span>
                        <button class="quantity-btn increase" aria-label="Aumentar quantidade de ${item.name}">+</button>
                        <button class="remove-item" aria-label="Remover ${item.name} do carrinho">
                            <svg aria-hidden="true" width="20" height="20" viewBox="0 0 24 24">
                                <path fill="currentColor" d="M7 21q-.825 0-1.412-.587Q5 19.825 5 19V6H4V4h5V3h6v1h5v2h-1v13q0 .825-.587 1.413Q17.825 21 17 21ZM17 6H7v13h10ZM9 17h2V8H9Zm4 0h2V8h-2ZM7 6v13Z"/>
                            </svg>
                        </button>
                    </div>
                `;
                
                // Controles de quantidade
                itemElement.querySelector('.decrease').addEventListener('click', () => {
                    this.removeFromCart(item.name, item.quantity === 1);
                });
                
                itemElement.querySelector('.increase').addEventListener('click', () => {
                    this.addToCart(item);
                });
                
                itemElement.querySelector('.remove-item').addEventListener('click', () => {
                    this.removeFromCart(item.name, true);
                });
                
                itemsContainer.appendChild(itemElement);
            });
        }
        
        this.updateTotals();
    }
    
    formatPrice(price) {
        return price.toFixed(2).replace('.', ',');
    }
    
    updateTotals() {
        const totalItems = this.cart.reduce((sum, item) => sum + item.quantity, 0);
        const totalPrice = this.cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
        
        // Atualiza o modal
        this.cartModal.querySelector('#modal-total-items').textContent = totalItems;
        this.cartModal.querySelector('#modal-total-price').textContent = `R$ ${this.formatPrice(totalPrice)}`;
        
        // Atualiza o footer se existir
        const footerItems = document.getElementById('footer-total-items');
        const footerPrice = document.getElementById('footer-total-price');
        if (footerItems) footerItems.textContent = totalItems;
        if (footerPrice) footerPrice.textContent = `R$ ${this.formatPrice(totalPrice)}`;
    }

    showAddedToCartFeedback(productName) {
        // Remove feedback existente
        const existingFeedback = document.querySelector('.cart-feedback');
        if (existingFeedback) existingFeedback.remove();

        const feedback = document.createElement('div');
        feedback.className = 'cart-feedback';
        feedback.setAttribute('role', 'status');
        feedback.innerHTML = `
            <span class="feedback-icon">✔</span>
            <span class="feedback-text">${productName} adicionado ao carrinho</span>
        `;
        document.body.appendChild(feedback);
        
        // Força o recálculo do layout para animação
        void feedback.offsetWidth;
        feedback.classList.add('show');
        
        setTimeout(() => {
            feedback.classList.remove('show');
            setTimeout(() => feedback.remove(), 300);
        }, 2000);
    }

    async confirmOrder() {
        if (this.cart.length === 0) {
            this.showAlert('Seu carrinho está vazio!', 'error');
            return;
        }
        
        const confirm = await this.showConfirmationDialog(
            'Confirmar Pedido',
            `Você está prestes a confirmar um pedido com ${this.cart.reduce((sum, item) => sum + item.quantity, 0)} itens. Deseja continuar?`
        );
        
        if (!confirm) return;
        
        try {
            const submitButton = this.cartModal.querySelector('.confirm-button');
            const originalText = submitButton.textContent;
            submitButton.disabled = true;
            submitButton.innerHTML = '<span class="spinner"></span> Processando...';
            
            const success = await this.sendOrderToServer();
            
            if (success) {
                this.showAlert('Pedido confirmado! Obrigado por sua compra.', 'success');
                this.cart = [];
                this.updateCart();
                this.toggleCart();
            }
        } catch (error) {
            console.error('Erro ao confirmar pedido:', error);
            this.showAlert('Erro ao confirmar pedido. Por favor, tente novamente.', 'error');
        } finally {
            const submitButton = this.cartModal.querySelector('.confirm-button');
            if (submitButton) {
                submitButton.disabled = false;
                submitButton.textContent = originalText;
            }
        }
    }

    async showConfirmationDialog(title, message) {
        return new Promise((resolve) => {
            const dialog = document.createElement('div');
            dialog.className = 'confirmation-dialog';
            dialog.innerHTML = `
                <div class="dialog-content">
                    <h3>${title}</h3>
                    <p>${message}</p>
                    <div class="dialog-buttons">
                        <button class="dialog-cancel">Cancelar</button>
                        <button class="dialog-confirm">Confirmar</button>
                    </div>
                </div>
            `;
            
            const overlay = document.createElement('div');
            overlay.className = 'dialog-overlay';
            
            document.body.appendChild(overlay);
            document.body.appendChild(dialog);
            
            dialog.querySelector('.dialog-cancel').addEventListener('click', () => {
                dialog.remove();
                overlay.remove();
                resolve(false);
            });
            
            dialog.querySelector('.dialog-confirm').addEventListener('click', () => {
                dialog.remove();
                overlay.remove();
                resolve(true);
            });
            
            // Foco no primeiro botão
            setTimeout(() => {
                dialog.querySelector('.dialog-cancel').focus();
            }, 50);
        });
    }

    showAlert(message, type = 'info') {
        // Remove alerta existente
        const existingAlert = document.querySelector('.alert');
        if (existingAlert) existingAlert.remove();

        const alertDiv = document.createElement('div');
        alertDiv.className = `alert ${type}`;
        alertDiv.setAttribute('role', 'alert');
        alertDiv.innerHTML = `
            <span class="alert-icon">${type === 'error' ? '⚠' : type === 'success' ? '✓' : 'i'}</span>
            <span class="alert-message">${message}</span>
        `;
        document.body.appendChild(alertDiv);
        
        // Força o recálculo do layout para animação
        void alertDiv.offsetWidth;
        alertDiv.classList.add('show');
        
        setTimeout(() => {
            alertDiv.classList.remove('show');
            setTimeout(() => alertDiv.remove(), 300);
        }, 3000);
    }

    async sendOrderToServer() {
        // Simulação de requisição assíncrona
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                try {
                    // Em produção, aqui seria uma chamada fetch/axios real
                    const orderData = {
                        items: this.cart,
                        total: this.cart.reduce((sum, item) => sum + (item.price * item.quantity), 0),
                        timestamp: new Date().toISOString()
                    };
                    console.log('Pedido enviado:', orderData);
                    
                    // Simulação de resposta aleatória (80% de sucesso)
                    const success = Math.random() > 0.2;
                    if (success) {
                        resolve(true);
                    } else {
                        throw new Error('Erro simulado no servidor');
                    }
                } catch (error) {
                    reject(error);
                }
            }, 1500);
        });
    }
}

// Inicialização
document.addEventListener('DOMContentLoaded', () => {
    window.fastFoodCart = new FastFoodCart();
    const cart = new FastFoodCart();
    window.fastFoodCart = cart; // Torna acessível globalmente
    
    
    // Adiciona estilos dinâmicos se não existirem
    if (!document.getElementById('cart-styles')) {
        const styleElement = document.createElement('style');
        styleElement.id = 'cart-styles';
        styleElement.textContent = `
            .cart-modal {
                /* Seus estilos existentes */
                transition: opacity 0.3s ease, transform 0.3s ease;
                opacity: 0;
                transform: translate(-50%, -48%);
            }
            
            .cart-modal.active {
                opacity: 1;
                transform: translate(-50%, -50%);
            }
            
            .modal-overlay {
                transition: opacity 0.3s ease;
            }
            
            .cart-feedback {
                position: fixed;
                bottom: 20px;
                left: 50%;
                transform: translateX(-50%);
                background: #4CAF50;
                color: white;
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
            
            .cart-feedback.show {
                opacity: 1;
            }
            
            .alert {
                position: fixed;
                top: 20px;
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
            
            .alert.show {
                opacity: 1;
            }
            
            .alert.info {
                background: #2196F3;
                color: white;
            }
            
            .alert.success {
                background: #4CAF50;
                color: white;
            }
            
            .alert.error {
                background: #F44336;
                color: white;
            }
            
            .confirmation-dialog {
                position: fixed;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
                background: white;
                padding: 20px;
                border-radius: 8px;
                box-shadow: 0 4px 20px rgba(0,0,0,0.2);
                z-index: 1200;
                width: 90%;
                max-width: 400px;
            }
            
            .dialog-overlay {
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(0,0,0,0.5);
                z-index: 1100;
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
});