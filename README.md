# Recipe Generator

Recipe Generator is a web application built with ASP.NET Core that allows users to generate delicious recipes using the power of AI. Users can enter ingredients or describe the type of dish they want, and the application will generate a complete recipe with instructions and an image of the final dish.

# https://recipe-genie.xyz/

## Features

- **AI-powered Recipe Generation:** The application uses the OpenAI GPT-4 language model to generate detailed recipes based on user input.
- **Image Generation:** In addition to the recipe instructions, the application generates a realistic image of the final dish using the DALL-E image generation model.
- **User Authentication:** Users can create accounts and authenticate with the application, allowing them to save and access their generated recipes.
- **Recipe History:** Authenticated users can view a history of their previously generated recipes.
- **Cloud Storage:** Generated recipe images are securely stored in the Cloudflare R2 object storage service, ensuring reliable access and scalability.

## Technologies Used

- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- OpenAI API
- DALL-E Image Generation
- Cloudflare R2 Object Storage
- Amazon S3 SDK

## Getting Started

To run the Recipe Generator application locally, follow these steps:

1. Clone the repository:
   ```bash
   git clone https://github.com/nealmick/recipe-genie.git
   ```

#### Set up the required environment variables:

- OPENAI_API_KEY: Your OpenAI API key for accessing the GPT-4 and DALL-E models.
- CLOUDFLARE_R2_ACCESS_KEY: Your Cloudflare R2 access key for object storage.
- CLOUDFLARE_R2_SECRET_KEY: Your Cloudflare R2 secret key for object storage.
- CLOUDFLARE_R2_BUCKET_NAME: The name of your Cloudflare R2 bucket.
- CLOUDFLARE_R2_ENDPOINT_URL: The endpoint URL for your Cloudflare R2 bucket.
- CONNECTION_STRING: The connection string for your PostgreSQL database.

#### Build the application:

dotnet build

#### Run the application:

dotnet run

Open your web browser and navigate to http://localhost:5000 (or the appropriate URL and port displayed in your console).

#### Contributing

Contributions to the Recipe Generator project are welcome! If you find any issues or want to add new features, please open an issue or submit a pull request with your changes.
