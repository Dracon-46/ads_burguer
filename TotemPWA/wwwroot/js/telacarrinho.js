document.addEventListener("DOMContentLoaded", () => {
    const quantityButtons = document.querySelectorAll(".quantity button");
    const totalItems = document.getElementById("total-items");
    const totalPrice = document.getElementById("total-price");

    quantityButtons.forEach(button => {
        button.addEventListener("click", (event) => {
            const quantitySpan = event.target.parentElement.querySelector("span");
            let quantity = parseInt(quantitySpan.innerText);
            const priceElement = event.target.closest(".item").querySelector(".price");
            let price = parseFloat(priceElement.innerText.replace("R$", "").replace(",", "."));
            
            if (event.target.innerText === "+") {
                quantity++;
            } else if (event.target.innerText === "-" && quantity > 1) {
                quantity--;
            }
            
            quantitySpan.innerText = quantity;
            updateTotal();
        });
    });

    function updateTotal() {
        let totalItemsCount = 0;
        let totalPriceValue = 0;

        document.querySelectorAll(".item").forEach(item => {
            let quantity = parseInt(item.querySelector(".quantity span").innerText);
            let price = parseFloat(item.querySelector(".price").innerText.replace("R$", "").replace(",", "."));
            totalItemsCount += quantity;
            totalPriceValue += quantity * price;
        });

        totalItems.innerText = totalItemsCount;
        totalPrice.innerText = `R$ ${totalPriceValue.toFixed(2).replace(".", ",")}`;
    }
});
