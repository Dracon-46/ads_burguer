document.addEventListener("DOMContentLoaded", function () {
    const ingredientes = document.querySelectorAll(".ingrediente");
    let totalItens = 0;
    let totalPedido = 0;

    function atualizarResumo() {
        document.querySelector(".total-itens").textContent = `Total de itens: ${totalItens}`;
        document.querySelector(".total-pedidos").textContent = `Total do Pedido: R$ ${totalPedido.toFixed(2).replace(".", ",")}`;
    }

    ingredientes.forEach(ingrediente => {
        const btnMais = ingrediente.querySelector(".incrementar");
        const btnMenos = ingrediente.querySelector(".decrementar");
        const quantidadeSpan = ingrediente.querySelector(".quantidade");
        let quantidade = 0;
        const preco = parseFloat(ingrediente.dataset.preco);

        btnMais.addEventListener("click", function () {
            if (quantidade < 5) { 
                quantidade++;
                quantidadeSpan.textContent = quantidade;
                totalItens++;
                totalPedido += preco;
                atualizarResumo();
            }
        });

        btnMenos.addEventListener("click", function () {
            if (quantidade > 0) {
                quantidade--;
                quantidadeSpan.textContent = quantidade;
                totalItens--;
                totalPedido -= preco;
                atualizarResumo();
            }
        });
    });
});
