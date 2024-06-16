document
  .getElementById("recipeForm")
  .addEventListener("submit", function (event) {
    event.preventDefault(); // Prevent default to manipulate before submitting
    const overlay = document.getElementById("overlay");
    const overlayContent = document.getElementById("overlay-content");
    const loadingImage = document.getElementById("loading-image");
    loadingImage.style.display = "block"; // Show loading image
    overlayContent.style.display = "block";
    overlay.classList.add("active"); // Make overlay active
    // Add a slight delay before showing the overlay content
    setTimeout(() => {
      overlayContent.classList.add("show");
      setTimeout(startCountup, 1000); // Start typing after 1 second
    }, 200);
    this.submit(); // Submit the form after showing the overlay
  });

function startCountup() {
  let seconds = 0;
  const countdownElement = document.getElementById("countdown");
  const progressText = document.getElementById("progressText");
  const steps = [
    "Preparing ingredients...",
    "Creating instructions...",
    "Generating thumbnail image...",
    "Finishing up...",
  ];
  let currentStep = 0;
  let isTyping = false;

  const typeMessage = (message) => {
    isTyping = true;
    let index = 0;
    const interval = setInterval(() => {
      if (index < message.length) {
        progressText.textContent += message[index++];
      } else {
        clearInterval(interval);
        isTyping = false;
      }
    }, 50); // Adjust the delay between characters (in milliseconds)
  };

  const deleteMessage = () => {
    if (!isTyping) {
      const interval = setInterval(() => {
        if (progressText.textContent.length > 0) {
          progressText.textContent = progressText.textContent.slice(0, -1);
        } else {
          clearInterval(interval);
          typeMessage(steps[currentStep % steps.length]);
          currentStep++;
        }
      }, 30); // Adjust the delay between character deletions (in milliseconds)
    }
  };

  typeMessage(steps[currentStep]); // Start typing the first message
  currentStep++;

  const countupInterval = setInterval(() => {
    countdownElement.textContent = `${++seconds}`;
    if (seconds % 5 === 0) {
      deleteMessage();
    }
  }, 1000);
}
