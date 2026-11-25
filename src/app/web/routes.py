"""Server-rendered browser entrypoint."""
from flask import Blueprint, render_template
web = Blueprint("web", __name__)

@web.get("/")
def index():
    """Render the self-contained quote/FNOL workflow shell."""
    return render_template("index.html")
