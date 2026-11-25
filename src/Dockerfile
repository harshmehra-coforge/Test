FROM python:3.12-slim
WORKDIR /app
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
COPY . .
ENV DATABASE_URL=sqlite:///insurance.db HOST=0.0.0.0 PORT=5000 FLASK_DEBUG=false
EXPOSE 5000
CMD ["python", "run.py"]
