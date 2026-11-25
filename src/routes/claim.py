"""
/claim endpoint: Submit FNOL and view claim status.
"""
from flask import Blueprint, request, jsonify

claim_bp = Blueprint('claim', __name__)

@claim_bp.route('/claim', methods=['POST'])
def submit_fnol():
    """Submit a new FNOL claim."""
    # TODO: Parse input, call claim_service, return result
    return jsonify({"claim_number": "TODO", "claim_status": "Submitted"})

@claim_bp.route('/claim/<int:claim_id>/status', methods=['GET'])
def claim_status(claim_id):
    """View claim status by claim ID."""
    # TODO: Fetch claim status
    return jsonify({"claim_status": "TODO"})
