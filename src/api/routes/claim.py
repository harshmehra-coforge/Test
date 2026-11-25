"""
/claim endpoint: Submit FNOL and view claim status.
"""
from flask import Blueprint, request, jsonify

claim_bp = Blueprint('claim', __name__, url_prefix='/claim')

@claim_bp.route('', methods=['POST'])
def submit_claim():
    """Submit a new FNOL claim."""
    # TODO: Parse input, call claim service, return response
    return jsonify({"message": "TODO: Implement claim submission"}), 501

@claim_bp.route('/<claim_id>', methods=['GET'])
def get_claim(claim_id):
    """Get claim status/details by claim_id."""
    # TODO: Fetch claim by ID, return status/details
    return jsonify({"message": "TODO: Implement claim status endpoint"}), 501
