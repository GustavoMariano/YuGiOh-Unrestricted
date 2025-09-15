window.dragDropHelper = {
    setCardData: function (event, cardId) {
        event.dataTransfer.setData("text/plain", cardId);
    },
    getCardData: function (event) {
        return event.dataTransfer.getData("text/plain");
    }
};
